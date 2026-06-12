using LeaveManagement.API.Data;
using LeaveManagement.API.DTOs;
using LeaveManagement.API.Models;
using LeaveManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRole.Employee + "," + UserRole.Manager + "," + UserRole.Admin)]
public class LeaveRequestsController(LeaveManagementDbContext context, LeaveRequestValidator validator) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = UserRole.Manager + "," + UserRole.Admin)]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetLeaveRequests()
    {
        return await VisibleRequests().OrderByDescending(lr => lr.CreatedDate).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeaveRequest>> GetLeaveRequest(int id)
    {
        var request = await VisibleRequests().FirstOrDefaultAsync(lr => lr.RequestID == id);
        return request is null ? NotFound(new { message = "Leave request not found." }) : request;
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetHistory()
    {
        return await VisibleRequests().OrderByDescending(lr => lr.CreatedDate).ToListAsync();
    }

    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> FilterRequests([FromQuery] string? status, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        var query = VisibleRequests();

        if (!string.IsNullOrWhiteSpace(status))
        {
            if (!LeaveStatus.IsValid(status))
            {
                return BadRequest(new { message = "Status must be Pending, Approved, or Rejected." });
            }
            query = query.Where(lr => lr.Status == status);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(lr => lr.FromDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(lr => lr.ToDate <= toDate.Value);
        }

        return await query.OrderByDescending(lr => lr.CreatedDate).ToListAsync();
    }

    [HttpGet("pending")]
    [Authorize(Roles = UserRole.Manager + "," + UserRole.Admin)]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetPendingRequests()
    {
        return await VisibleRequests().Where(lr => lr.Status == LeaveStatus.Pending).OrderBy(lr => lr.FromDate).ToListAsync();
    }

    [HttpPost]
    [Authorize(Roles = UserRole.Employee)]
    public async Task<ActionResult<LeaveRequest>> CreateLeaveRequest(LeaveRequestCreateDto dto)
    {
        var currentEmployeeId = GetCurrentEmployeeId();
        if (currentEmployeeId is null)
        {
            return Unauthorized(new { message = "Could not determine the signed-in employee." });
        }

        dto.EmployeeID = currentEmployeeId.Value;
        var employee = await context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.EmployeeID == currentEmployeeId.Value && e.IsActive);
        if (employee?.ManagerID is null)
        {
            return BadRequest(new { message = "Your profile must have an assigned manager before you can submit leave requests." });
        }

        var errors = await validator.ValidateCreateAsync(dto);
        if (errors.Count > 0)
        {
            return BadRequest(new { message = "Validation failed.", errors });
        }

        var request = new LeaveRequest
        {
            EmployeeID = dto.EmployeeID,
            LeaveTypeID = dto.LeaveTypeID,
            ManagerID = employee.ManagerID.Value,
            FromDate = dto.FromDate,
            ToDate = dto.ToDate,
            NumberOfDays = LeaveRequestValidator.CalculateNumberOfDays(dto.FromDate, dto.ToDate),
            Reason = dto.Reason.Trim(),
            Status = LeaveStatus.Pending,
            CreatedDate = DateTime.UtcNow
        };

        context.LeaveRequests.Add(request);
        await context.SaveChangesAsync();
        await context.Entry(request).Reference(lr => lr.Employee).LoadAsync();
        await context.Entry(request).Reference(lr => lr.LeaveType).LoadAsync();
        return CreatedAtAction(nameof(GetLeaveRequest), new { id = request.RequestID }, request);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = UserRole.Manager + "," + UserRole.Admin)]
    public async Task<IActionResult> UpdateLeaveRequest(int id, LeaveRequestUpdateDto dto)
    {
        var request = await context.LeaveRequests.FindAsync(id);
        if (request is null)
        {
            return NotFound(new { message = "Leave request not found." });
        }

        if (!CanManageRequest(request))
        {
            return Forbid();
        }

        var errors = await validator.ValidateCreateAsync(dto, dto.Status);
        if (errors.Count > 0)
        {
            return BadRequest(new { message = "Validation failed.", errors });
        }

        request.EmployeeID = dto.EmployeeID;
        request.LeaveTypeID = dto.LeaveTypeID;
        request.FromDate = dto.FromDate;
        request.ToDate = dto.ToDate;
        request.NumberOfDays = LeaveRequestValidator.CalculateNumberOfDays(dto.FromDate, dto.ToDate);
        request.Reason = dto.Reason.Trim();
        request.Status = dto.Status;
        request.ManagerComments = dto.ManagerComments;

        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = UserRole.Manager + "," + UserRole.Admin)]
    public async Task<IActionResult> DeleteLeaveRequest(int id)
    {
        var request = await context.LeaveRequests.FindAsync(id);
        if (request is null)
        {
            return NotFound(new { message = "Leave request not found." });
        }

        if (!CanManageRequest(request))
        {
            return Forbid();
        }

        context.LeaveRequests.Remove(request);
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = UserRole.Manager + "," + UserRole.Admin)]
    public async Task<IActionResult> ApproveRequest(int id, ManagerDecisionDto dto)
    {
        return await UpdateStatus(id, LeaveStatus.Approved, dto.ManagerComments);
    }

    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = UserRole.Manager + "," + UserRole.Admin)]
    public async Task<IActionResult> RejectRequest(int id, ManagerDecisionDto dto)
    {
        return await UpdateStatus(id, LeaveStatus.Rejected, dto.ManagerComments);
    }

    private async Task<IActionResult> UpdateStatus(int id, string status, string? managerComments)
    {
        var request = await context.LeaveRequests.FindAsync(id);
        if (request is null)
        {
            return NotFound(new { message = "Leave request not found." });
        }

        if (!CanManageRequest(request))
        {
            return Forbid();
        }

        request.Status = status;
        request.ManagerComments = managerComments;
        await context.SaveChangesAsync();
        return NoContent();
    }

    private bool CanManageRequest(LeaveRequest request)
    {
        if (User.IsInRole(UserRole.Admin))
        {
            return true;
        }

        var managerId = GetCurrentEmployeeId();
        return managerId is not null && request.ManagerID == managerId.Value;
    }

    private IQueryable<LeaveRequest> VisibleRequests()
    {
        var query = BaseQuery();
        if (User.IsInRole(UserRole.Manager))
        {
            var managerId = GetCurrentEmployeeId();
            return managerId is null ? query.Where(_ => false) : query.Where(lr => lr.ManagerID == managerId.Value);
        }

        if (User.IsInRole(UserRole.Admin))
        {
            return query;
        }

        var employeeId = GetCurrentEmployeeId();
        return employeeId is null ? query.Where(_ => false) : query.Where(lr => lr.EmployeeID == employeeId.Value);
    }

    private int? GetCurrentEmployeeId()
    {
        var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("uid")
            ?? User.FindFirstValue("nameid");

        return int.TryParse(employeeId, out var id) ? id : null;
    }

    private IQueryable<LeaveRequest> BaseQuery()
    {
        return context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.AssignedManager)
            .Include(lr => lr.LeaveType)
            .AsNoTracking();
    }
}
