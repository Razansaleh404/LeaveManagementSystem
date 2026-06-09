using LeaveManagement.API.Data;
using LeaveManagement.API.DTOs;
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
                Role = employee.Role
            }
        };
    }
}
