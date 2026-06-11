namespace LeaveManagement.API.Models;

public static class UserRole
{
    public const string Employee = "Employee";
    public const string Manager = "Manager";

    public static bool IsValid(string? role)
    {
        return Normalize(role) is not null;
    }

    public static string? Normalize(string? role)
    {
        return role?.Trim().ToLowerInvariant() switch
        {
            "employee" => Employee,
            "manager" => Manager,
            _ => null
        };
    }
}
