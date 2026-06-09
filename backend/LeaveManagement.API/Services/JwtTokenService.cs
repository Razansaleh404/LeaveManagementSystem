using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LeaveManagement.API.Authentication;
using LeaveManagement.API.Models;
using Microsoft.Extensions.Options;

namespace LeaveManagement.API.Services;

public class JwtTokenService(IOptions<JwtSettings> options)
{
    private readonly JwtSettings settings = options.Value;

    public (string Token, DateTime ExpiresAt) CreateToken(Employee employee)
    {
        var now = DateTime.UtcNow;
        var expiresAt = now.AddMinutes(settings.ExpirationMinutes);
        var header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };
        var payload = new Dictionary<string, object>
        {
            ["sub"] = employee.EmployeeID.ToString(),
            ["email"] = employee.Email,
            ["given_name"] = employee.FirstName,
            ["family_name"] = employee.LastName,
            ["role"] = employee.Role,
            ["iss"] = settings.Issuer,
            ["aud"] = settings.Audience,
            ["iat"] = new DateTimeOffset(now).ToUnixTimeSeconds(),
            ["exp"] = new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
        };

        var encodedHeader = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header));
        var encodedPayload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));
        var unsignedToken = $"{encodedHeader}.{encodedPayload}";
        var signature = CreateSignature(unsignedToken);
        return ($"{unsignedToken}.{signature}", expiresAt);
    }

    public ClaimsPrincipal? ValidateToken(string token)
    {
        var tokenParts = token.Split('.');
        if (tokenParts.Length != 3)
        {
            return null;
        }

        var unsignedToken = $"{tokenParts[0]}.{tokenParts[1]}";
        var expectedSignature = CreateSignature(unsignedToken);
        if (!FixedTimeEquals(expectedSignature, tokenParts[2]))
        {
            return null;
        }

        Dictionary<string, JsonElement>? payload;
        try
        {
            payload = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(Base64UrlDecode(tokenParts[1]));
        }
        catch (JsonException)
        {
            return null;
        }
        catch (FormatException)
        {
            return null;
        }

        if (payload is null
            || !HasStringValue(payload, "iss", settings.Issuer)
            || !HasStringValue(payload, "aud", settings.Audience)
            || !payload.TryGetValue("exp", out var expElement)
            || !payload.TryGetValue("sub", out var subElement)
            || !payload.TryGetValue("email", out var emailElement)
            || !payload.TryGetValue("role", out var roleElement))
        {
            return null;
        }

        DateTimeOffset expiresAt;
        try
        {
            expiresAt = DateTimeOffset.FromUnixTimeSeconds(expElement.GetInt64());
        }
        catch (FormatException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }

        if (expiresAt <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        var sub = subElement.GetString();
        var email = emailElement.GetString();
        var role = roleElement.GetString();
        if (string.IsNullOrWhiteSpace(sub) || string.IsNullOrWhiteSpace(email) || !UserRole.IsValid(role))
        {
            return null;
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, sub),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role!),
            new(ClaimTypes.Name, email)
        };

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "Jwt"));
    }

    private string CreateSignature(string unsignedToken)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(settings.Secret));
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken)));
    }

    private static bool HasStringValue(Dictionary<string, JsonElement> payload, string key, string expectedValue)
    {
        return payload.TryGetValue(key, out var value) && value.GetString() == expectedValue;
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        var padding = 4 - base64.Length % 4;
        if (padding < 4)
        {
            base64 += new string('=', padding);
        }
        return Convert.FromBase64String(base64);
    }

    private static bool FixedTimeEquals(string first, string second)
    {
        if (first.Length != second.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(first), Encoding.UTF8.GetBytes(second));
    }
}
