using LeaveManagement.API.Data;
using LeaveManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController(LeaveManagementDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
    {
        return await context.Employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName).ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        var employee = await context.Employees.FindAsync(id);
        return employee is null ? NotFound(new { message = "Employee not found." }) : employee;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Employee>>> SearchEmployees([FromQuery] string term)
    {
        term = term?.Trim() ?? string.Empty;
        return await context.Employees
            .Where(e => e.FirstName.Contains(term) || e.LastName.Contains(term) || e.Email.Contains(term))
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
    {
        if (await context.Employees.AnyAsync(e => e.Email == employee.Email))
        {
            return BadRequest(new { message = "Email must be unique." });
        }

        context.Employees.Add(employee);
        await context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeID }, employee);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, Employee employee)
    {
        if (id != employee.EmployeeID)
        {
            return BadRequest(new { message = "Employee ID in the URL must match the request body." });
        }

        if (await context.Employees.AnyAsync(e => e.Email == employee.Email && e.EmployeeID != id))
        {
            return BadRequest(new { message = "Email must be unique." });
        }

        if (!await context.Employees.AnyAsync(e => e.EmployeeID == id))
        {
            return NotFound(new { message = "Employee not found." });
        }

        context.Entry(employee).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound(new { message = "Employee not found." });
        }

        context.Employees.Remove(employee);
        await context.SaveChangesAsync();
        return NoContent();
    }
}
