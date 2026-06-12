using LeaveManagement.API.Data;
using LeaveManagement.API.DTOs;
using LeaveManagement.API.Models;
using LeaveManagement.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    LeaveManagementDbContext context,
    PasswordService passwordService,
    JwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();
        var employee = await context.Employees.FirstOrDefaultAsync(e => e.Email.ToLower() == normalizedEmail && e.IsActive);
        if (employee is null || !passwordService.VerifyPassword(request.Password, employee.PasswordHash, employee.PasswordSalt))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        return CreateLoginResponse(employee);
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register(RegisterRequestDto request)
    {
        var normalizedEmail = request.Email.Trim().ToLower();
        if (await context.Employees.AnyAsync(e => e.Email.ToLower() == normalizedEmail))
        {
            return Conflict(new { message = "Email already exists." });
        }

        var managerExists = await context.Employees.AnyAsync(e => e.EmployeeID == request.ManagerID && e.Role == UserRole.Manager && e.IsActive);
        if (!managerExists)
        {
            return BadRequest(new { message = "Choose an active manager for this employee account." });
        }

        var password = passwordService.HashPassword(request.Password);
        var employee = new Employee
        {
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            Department = request.Department.Trim(),
            Role = UserRole.Employee,
            ManagerID = request.ManagerID,
            PasswordHash = password.Hash,
            PasswordSalt = password.Salt,
            IsActive = true
        };

        context.Employees.Add(employee);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(Login), CreateLoginResponse(employee));
    }

    [HttpGet("managers")]
    public async Task<ActionResult<IEnumerable<AuthManagerDto>>> GetManagers()
    {
        return await context.Employees
            .Where(e => e.Role == UserRole.Manager && e.IsActive)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Select(e => new AuthManagerDto
            {
                EmployeeID = e.EmployeeID,
                FirstName = e.FirstName,
                LastName = e.LastName,
                Department = e.Department
            })
            .ToListAsync();
    }

    private LoginResponseDto CreateLoginResponse(Employee employee)
    {
        var (token, expiresAt) = jwtTokenService.CreateToken(employee);
        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = new AuthUserDto
            {
                EmployeeID = employee.EmployeeID,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Department = employee.Department,
                Role = employee.Role,
                ManagerID = employee.ManagerID
            }
        };
    }
}
