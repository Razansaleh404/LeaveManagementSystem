using LeaveManagement.API.Data;
using LeaveManagement.API.DTOs;
using LeaveManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(LeaveManagementDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetEmployees() => await context.Employees
        .OrderBy(e => e.FullName)
        .Select(e => new EmployeeDto
        {
            EmployeeID = e.EmployeeID,
            EmployeeCode = e.EmployeeCode,
            FullName = e.FullName,
            Email = e.Email,
            Department = e.Department,
            IsActive = e.IsActive
        })
        .ToListAsync();

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
    {
        var employee = await context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound(new { message = "Employee was not found." });
        }

        return ToDto(employee);
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<EmployeeDto>>> SearchEmployees([FromQuery] string? term)
    {
        term = term?.Trim() ?? string.Empty;
        return await context.Employees
            .Where(e => string.IsNullOrEmpty(term) || e.EmployeeCode.Contains(term) || e.FullName.Contains(term))
            .OrderBy(e => e.FullName)
            .Select(e => new EmployeeDto
        {
            EmployeeID = e.EmployeeID,
            EmployeeCode = e.EmployeeCode,
            FullName = e.FullName,
            Email = e.Email,
            Department = e.Department,
            IsActive = e.IsActive
        })
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> CreateEmployee(EmployeeCreateUpdateDto dto)
    {
        var duplicate = await context.Employees.AnyAsync(e => e.EmployeeCode == dto.EmployeeCode || e.Email == dto.Email);
        if (duplicate)
        {
            return BadRequest(new { message = "Employee code or email already exists." });
        }

        var employee = new Employee
        {
            EmployeeCode = dto.EmployeeCode.Trim(),
            FullName = dto.FullName.Trim(),
            Email = dto.Email.Trim(),
            Department = dto.Department.Trim(),
            IsActive = dto.IsActive
        };

        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeID }, ToDto(employee));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, EmployeeCreateUpdateDto dto)
    {
        var employee = await context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound(new { message = "Employee was not found." });
        }

        var duplicate = await context.Employees.AnyAsync(e => e.EmployeeID != id && (e.EmployeeCode == dto.EmployeeCode || e.Email == dto.Email));
        if (duplicate)
        {
            return BadRequest(new { message = "Employee code or email already exists." });
        }

        employee.EmployeeCode = dto.EmployeeCode.Trim();
        employee.FullName = dto.FullName.Trim();
        employee.Email = dto.Email.Trim();
        employee.Department = dto.Department.Trim();
        employee.IsActive = dto.IsActive;

        await context.SaveChangesAsync();
        return Ok(ToDto(employee));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound(new { message = "Employee was not found." });
        }

        var hasRequests = await context.LeaveRequests.AnyAsync(r => r.EmployeeID == id);
        if (hasRequests)
        {
            employee.IsActive = false;
        }
        else
        {
            context.Employees.Remove(employee);
        }

        await context.SaveChangesAsync();
        return NoContent();
    }

    private static EmployeeDto ToDto(Employee employee) => new()
    {
        EmployeeID = employee.EmployeeID,
        EmployeeCode = employee.EmployeeCode,
        FullName = employee.FullName,
        Email = employee.Email,
        Department = employee.Department,
        IsActive = employee.IsActive
    };
}
