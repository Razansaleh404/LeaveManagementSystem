using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LeaveManagement.API.Authentication;
using LeaveManagement.API.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LeaveManagement.API.Services;

public class JwtTokenService(IOptions<JwtSettings> options)
{
    private readonly JwtSettings settings = options.Value;

    public (string Token, DateTime ExpiresAt) CreateToken(Employee employee)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(settings.DurationInMinutes);
        var employeeId = employee.EmployeeID.ToString();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, employee.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, employee.Email),
            new Claim("uid", employeeId),
            new Claim("nameid", employeeId),
            new Claim("unique_name", employee.Email),
            new Claim("given_name", employee.FirstName),
            new Claim("family_name", employee.LastName),
            new Claim("role", employee.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
