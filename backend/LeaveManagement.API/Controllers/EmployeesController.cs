using LeaveManagement.API.Data;
using LeaveManagement.API.DTOs;
using LeaveManagement.API.Models;
using LeaveManagement.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = UserRole.Manager)]
public class EmployeesController(LeaveManagementDbContext context, ILogger<EmployeesController> logger, PasswordService passwordService) : ControllerBase
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
            .Where(e => e.FirstName.Contains(term) || e.LastName.Contains(term) || e.Email.Contains(term) || e.Department.Contains(term))
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> CreateEmployee(EmployeeCreateUpdateDto request)
    {
        if (!IsValidEmployeeRequest(request))
        {
            return BadRequest(new { message = "Invalid employee data." });
        }

        var normalizedEmail = request.Email.Trim().ToLower();
        if (await EmailExistsAsync(normalizedEmail))
        {
            return Conflict(new { message = "Email already exists." });
        }

        var employee = new Employee
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Department = request.Department.Trim(),
            Role = UserRole.Normalize(request.Role)!,
            IsActive = request.IsActive
        };

        var password = passwordService.HashPassword(request.Password ?? "Password123!");
        employee.PasswordHash = password.Hash;
        employee.PasswordSalt = password.Salt;

        context.Employees.Add(employee);

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error while creating employee with email {Email}.", employee.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected database error occurred while saving the employee." });
        }

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.EmployeeID }, employee);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateEmployee(int id, EmployeeCreateUpdateDto request)
    {
        if (!IsValidEmployeeRequest(request))
        {
            return BadRequest(new { message = "Invalid employee data." });
        }

        var employee = await context.Employees.FindAsync(id);
        if (employee is null)
        {
            return NotFound(new { message = "Employee not found." });
        }

        var normalizedEmail = request.Email.Trim().ToLower();
        if (await EmailExistsAsync(normalizedEmail, id))
        {
            return Conflict(new { message = "Email already exists." });
        }

        employee.FirstName = request.FirstName.Trim();
        employee.LastName = request.LastName.Trim();
        employee.Email = request.Email.Trim();
        employee.Department = request.Department.Trim();
        employee.Role = UserRole.Normalize(request.Role)!;
        employee.IsActive = request.IsActive;
        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            var password = passwordService.HashPassword(request.Password);
            employee.PasswordHash = password.Hash;
            employee.PasswordSalt = password.Salt;
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error while updating employee {EmployeeID}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected database error occurred while saving the employee." });
        }

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

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database error while deleting employee {EmployeeID}.", id);
            return BadRequest(new { message = "Employee cannot be deleted while related leave requests exist." });
        }

        return NoContent();
    }

    private static bool IsValidEmployeeRequest(EmployeeCreateUpdateDto request)
    {
        return !string.IsNullOrWhiteSpace(request.FirstName)
            && !string.IsNullOrWhiteSpace(request.LastName)
            && !string.IsNullOrWhiteSpace(request.Email)
            && !string.IsNullOrWhiteSpace(request.Department)
            && UserRole.IsValid(request.Role)
            && new EmailAddressAttribute().IsValid(request.Email);
    }

    private Task<bool> EmailExistsAsync(string normalizedEmail, int? excludeEmployeeId = null)
    {
        return context.Employees.AnyAsync(e => e.Email.ToLower() == normalizedEmail
            && (!excludeEmployeeId.HasValue || e.EmployeeID != excludeEmployeeId.Value));
    }
}
