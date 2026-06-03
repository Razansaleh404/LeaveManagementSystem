using LeaveManagement.API.Data;
using LeaveManagement.API.DTOs;
using LeaveManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Services;

public class LeaveRequestService(LeaveManagementDbContext context) : ILeaveRequestService
{
    public Task<List<LeaveRequestDto>> GetAllAsync() => ProjectRequests(context.LeaveRequests)
        .OrderByDescending(r => r.CreatedDate)
        .ToListAsync();

    public Task<LeaveRequestDto?> GetByIdAsync(int id) => ProjectRequests(context.LeaveRequests)
        .FirstOrDefaultAsync(r => r.RequestID == id);

    public Task<List<LeaveRequestDto>> GetPendingAsync() => FilterAsync(LeaveRequestStatus.Pending, null, null);

    public Task<List<LeaveRequestDto>> FilterAsync(string? status, DateTime? fromDate, DateTime? toDate)
    {
        var query = context.LeaveRequests.AsQueryable();

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(r => r.Status == status);
        }

        if (fromDate.HasValue)
        {
            var date = fromDate.Value.Date;
            query = query.Where(r => r.FromDate >= date);
        }

        if (toDate.HasValue)
        {
            var date = toDate.Value.Date;
            query = query.Where(r => r.ToDate <= date);
        }

        return ProjectRequests(query).OrderByDescending(r => r.CreatedDate).ToListAsync();
    }

    public async Task<(bool Success, string? Error, LeaveRequestDto? Request)> CreateAsync(LeaveRequestCreateDto dto)
    {
        var validation = await ValidateCreateAsync(dto);
        if (!validation.Success)
        {
            return (false, validation.Error, null);
        }

        var fromDate = dto.FromDate.Date;
        var toDate = dto.ToDate.Date;
        var request = new LeaveRequest
        {
            EmployeeID = dto.EmployeeID,
            LeaveTypeID = dto.LeaveTypeID,
            FromDate = fromDate,
            ToDate = toDate,
            NumberOfDays = CalculateInclusiveDays(fromDate, toDate),
            Reason = dto.Reason.Trim(),
            Status = LeaveRequestStatus.Pending,
            CreatedDate = DateTime.UtcNow
        };

        context.LeaveRequests.Add(request);
        await context.SaveChangesAsync();
        return (true, null, await GetByIdAsync(request.RequestID));
    }

    public Task<(bool Success, string? Error, LeaveRequestDto? Request)> ApproveAsync(int id, LeaveDecisionDto dto) =>
        SetStatusAsync(id, LeaveRequestStatus.Approved, dto.ManagerComments);

    public Task<(bool Success, string? Error, LeaveRequestDto? Request)> RejectAsync(int id, LeaveDecisionDto dto) =>
        SetStatusAsync(id, LeaveRequestStatus.Rejected, dto.ManagerComments);

    private async Task<(bool Success, string? Error)> ValidateCreateAsync(LeaveRequestCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Reason))
        {
            return (false, "Reason is required.");
        }

        var today = DateTime.UtcNow.Date;
        var fromDate = dto.FromDate.Date;
        var toDate = dto.ToDate.Date;

        if (fromDate < today || toDate < today)
        {
            return (false, "Leave dates cannot be in the past.");
        }

        if (toDate < fromDate)
        {
            return (false, "To date must be greater than or equal to from date.");
        }

        if (!await context.Employees.AnyAsync(e => e.EmployeeID == dto.EmployeeID && e.IsActive))
        {
            return (false, "The selected employee does not exist or is inactive.");
        }

        if (!await context.LeaveTypes.AnyAsync(t => t.LeaveTypeID == dto.LeaveTypeID))
        {
            return (false, "The selected leave type does not exist.");
        }

        return (true, null);
    }

    private async Task<(bool Success, string? Error, LeaveRequestDto? Request)> SetStatusAsync(int id, string status, string? comments)
    {
        var request = await context.LeaveRequests.FindAsync(id);
        if (request is null)
        {
            return (false, "Leave request was not found.", null);
        }

        if (request.Status != LeaveRequestStatus.Pending)
        {
            return (false, "Only pending leave requests can be approved or rejected.", null);
        }

        request.Status = status;
        request.ManagerComments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();
        await context.SaveChangesAsync();
        return (true, null, await GetByIdAsync(id));
    }

    private static int CalculateInclusiveDays(DateTime fromDate, DateTime toDate) =>
        (toDate.Date - fromDate.Date).Days + 1;

    private static IQueryable<LeaveRequestDto> ProjectRequests(IQueryable<LeaveRequest> query) => query.Select(r => new LeaveRequestDto
    {
        RequestID = r.RequestID,
        EmployeeID = r.EmployeeID,
        EmployeeName = r.Employee != null ? r.Employee.FullName : string.Empty,
        LeaveTypeID = r.LeaveTypeID,
        LeaveTypeName = r.LeaveType != null ? r.LeaveType.LeaveName : string.Empty,
        FromDate = r.FromDate,
        ToDate = r.ToDate,
        NumberOfDays = r.NumberOfDays,
        Reason = r.Reason,
        Status = r.Status,
        CreatedDate = r.CreatedDate,
        ManagerComments = r.ManagerComments
    });
}
