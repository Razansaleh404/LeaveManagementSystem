using System.Security.Cryptography;
using System.Text;

namespace LeaveManagement.API.Auth;

public class PasswordService
{
    public bool Verify(string password, string storedHash)
    {
        var parts = storedHash.Split(':', 2);
        if (parts.Length != 2)
        {
            return false;
        }

        var computedHash = Hash(password, parts[0]);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(storedHash),
            Encoding.UTF8.GetBytes(computedHash));
    }

    public static string Hash(string password, string salt)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(salt));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return $"{salt}:{Convert.ToBase64String(hash)}";
    }
}
