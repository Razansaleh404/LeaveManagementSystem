using LeaveManagement.API.Data;
using LeaveManagement.API.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaveTypesController(LeaveManagementDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LeaveTypeDto>>> GetLeaveTypes() => await context.LeaveTypes
        .OrderBy(t => t.LeaveName)
        .Select(t => new LeaveTypeDto { LeaveTypeID = t.LeaveTypeID, LeaveName = t.LeaveName })
        .ToListAsync();
}
