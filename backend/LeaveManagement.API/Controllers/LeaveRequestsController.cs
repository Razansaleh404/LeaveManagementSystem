using LeaveManagement.API.Auth;
using LeaveManagement.API.Constants;
using LeaveManagement.API.Data;
using LeaveManagement.API.DTOs;
using LeaveManagement.API.Models;
using LeaveManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LeaveRequestsController(LeaveManagementDbContext context, LeaveRequestValidator validator) : ControllerBase
{
    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetLeaveRequests()
    {
        return await BaseQuery().OrderByDescending(lr => lr.CreatedDate).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeaveRequest>> GetLeaveRequest(int id)
    {
        var request = await ScopedQuery().FirstOrDefaultAsync(lr => lr.RequestID == id);
        return request is null ? NotFound(new { message = "Leave request not found." }) : request;
    }

    [HttpGet("history")]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetHistory()
    {
        return await ScopedQuery().OrderByDescending(lr => lr.CreatedDate).ToListAsync();
    }

    [HttpGet("filter")]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> FilterRequests([FromQuery] string? status, [FromQuery] DateOnly? fromDate, [FromQuery] DateOnly? toDate)
    {
        var query = ScopedQuery();

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

    [Authorize(Roles = "Manager,Admin")]
    [HttpGet("pending")]
    public async Task<ActionResult<IEnumerable<LeaveRequest>>> GetPendingRequests()
    {
        return await BaseQuery().Where(lr => lr.Status == LeaveStatus.Pending).OrderBy(lr => lr.FromDate).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<LeaveRequest>> CreateLeaveRequest(LeaveRequestCreateDto dto)
    {
        var currentEmployeeId = User.GetEmployeeId();
        if (dto.EmployeeID != currentEmployeeId)
        {
            return Forbid();
        }

        var errors = await validator.ValidateCreateAsync(dto);
        if (errors.Count > 0)
        {
            return BadRequest(new { message = "Validation failed.", errors });
        }

        var request = new LeaveRequest
        {
            EmployeeID = currentEmployeeId,
            LeaveTypeID = dto.LeaveTypeID,
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

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLeaveRequest(int id, LeaveRequestUpdateDto dto)
    {
        var request = await context.LeaveRequests.FindAsync(id);
        if (request is null)
        {
            return NotFound(new { message = "Leave request not found." });
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

    [Authorize(Roles = AppRoles.Admin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLeaveRequest(int id)
    {
        var request = await context.LeaveRequests.FindAsync(id);
        if (request is null)
        {
            return NotFound(new { message = "Leave request not found." });
        }

        context.LeaveRequests.Remove(request);
        await context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPut("{id:int}/approve")]
    public async Task<IActionResult> ApproveRequest(int id, ManagerDecisionDto dto)
    {
        return await UpdateStatus(id, LeaveStatus.Approved, dto.ManagerComments);
    }

    [Authorize(Roles = "Manager,Admin")]
    [HttpPut("{id:int}/reject")]
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

        if (request.Status != LeaveStatus.Pending)
        {
            return BadRequest(new { message = "Only pending requests can be approved or rejected." });
        }

        request.Status = status;
        request.ManagerComments = managerComments;
        await context.SaveChangesAsync();
        return NoContent();
    }

    private IQueryable<LeaveRequest> ScopedQuery()
    {
        var query = BaseQuery();
        return User.IsInRole(AppRoles.Admin) ? query : query.Where(lr => lr.EmployeeID == User.GetEmployeeId());
    }

    private IQueryable<LeaveRequest> BaseQuery()
    {
        return context.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .AsNoTracking();
    }
}
