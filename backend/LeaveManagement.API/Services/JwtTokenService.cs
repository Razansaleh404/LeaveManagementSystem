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
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, employee.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, employee.Email),
            new Claim("uid", employee.EmployeeID.ToString()),
            new Claim(ClaimTypes.NameIdentifier, employee.EmployeeID.ToString()),
            new Claim(ClaimTypes.Name, employee.Email),
            new Claim(ClaimTypes.GivenName, employee.FirstName),
            new Claim(ClaimTypes.Surname, employee.LastName),
            new Claim(ClaimTypes.Role, employee.Role)
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
