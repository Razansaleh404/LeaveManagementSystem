using LeaveManagement.API.DTOs;
using LeaveManagement.API.Models;
using LeaveManagement.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveRequestsController(ILeaveRequestService leaveRequestService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetLeaveRequests() => await leaveRequestService.GetAllAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeaveRequestDto>> GetLeaveRequest(int id)
    {
        var request = await leaveRequestService.GetByIdAsync(id);
        if (request is null)
        {
            return NotFound(new { message = "Leave request was not found." });
        }

        return request;
    }

    [HttpGet("filter")]
    public async Task<ActionResult<List<LeaveRequestDto>>> FilterLeaveRequests([FromQuery] string? status, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        if (!string.IsNullOrWhiteSpace(status) && !LeaveRequestStatus.All.Contains(status))
        {
            return BadRequest(new { message = "Status must be Pending, Approved, or Rejected." });
        }

        if (fromDate.HasValue && toDate.HasValue && toDate.Value.Date < fromDate.Value.Date)
        {
            return BadRequest(new { message = "To date must be greater than or equal to from date." });
        }

        return await leaveRequestService.FilterAsync(status, fromDate, toDate);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<List<LeaveRequestDto>>> GetPendingRequests() => await leaveRequestService.GetPendingAsync();

    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> CreateLeaveRequest(LeaveRequestCreateDto dto)
    {
        var result = await leaveRequestService.CreateAsync(dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return CreatedAtAction(nameof(GetLeaveRequest), new { id = result.Request!.RequestID }, result.Request);
    }

    [HttpPut("{id:int}/approve")]
    public async Task<ActionResult<LeaveRequestDto>> ApproveLeaveRequest(int id, LeaveDecisionDto dto)
    {
        var result = await leaveRequestService.ApproveAsync(id, dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return result.Request!;
    }

    [HttpPut("{id:int}/reject")]
    public async Task<ActionResult<LeaveRequestDto>> RejectLeaveRequest(int id, LeaveDecisionDto dto)
    {
        var result = await leaveRequestService.RejectAsync(id, dto);
        if (!result.Success)
        {
            return BadRequest(new { message = result.Error });
        }

        return result.Request!;
    }
}
