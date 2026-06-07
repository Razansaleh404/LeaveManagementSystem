using LeaveManagement.API.Data;
using LeaveManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveTypesController(LeaveManagementDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LeaveType>>> GetLeaveTypes()
    {
        return await context.LeaveTypes.OrderBy(lt => lt.LeaveName).ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<LeaveType>> CreateLeaveType(LeaveType leaveType)
    {
        context.LeaveTypes.Add(leaveType);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetLeaveTypes), new { id = leaveType.LeaveTypeID }, leaveType);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateLeaveType(int id, LeaveType leaveType)
    {
        if (id != leaveType.LeaveTypeID)
        {
            return BadRequest(new { message = "Leave type ID in the URL must match the request body." });
        }

        if (!await context.LeaveTypes.AnyAsync(lt => lt.LeaveTypeID == id))
        {
            return NotFound(new { message = "Leave type not found." });
        }

        context.Entry(leaveType).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteLeaveType(int id)
    {
        var leaveType = await context.LeaveTypes.FindAsync(id);
        if (leaveType is null)
        {
            return NotFound(new { message = "Leave type not found." });
        }

        context.LeaveTypes.Remove(leaveType);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
