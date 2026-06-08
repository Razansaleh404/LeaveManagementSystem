using LeaveManagement.API.Auth;
using LeaveManagement.API.Data;
using LeaveManagement.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(LeaveManagementDbContext context, PasswordService passwordService, JwtTokenService jwtTokenService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginRequestDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        var user = await context.AppUsers
            .Include(u => u.Employee)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email && u.Employee.IsActive);

        if (user is null || !passwordService.Verify(dto.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = jwtTokenService.CreateToken(user);
        return new LoginResponseDto
        {
            Token = token.Token,
            ExpiresAt = token.ExpiresAt,
            Role = user.Role,
            EmployeeID = user.EmployeeID,
            Email = user.Email,
            FullName = $"{user.Employee.FirstName} {user.Employee.LastName}".Trim()
        };
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var employeeId = User.GetEmployeeId();
        var user = await context.AppUsers.Include(u => u.Employee).FirstOrDefaultAsync(u => u.EmployeeID == employeeId);
        if (user is null)
        {
            return Unauthorized(new { message = "User was not found." });
        }

        return new CurrentUserDto
        {
            EmployeeID = user.EmployeeID,
            Email = user.Email,
            FullName = $"{user.Employee.FirstName} {user.Employee.LastName}".Trim(),
            Role = user.Role
        };
    }
}
