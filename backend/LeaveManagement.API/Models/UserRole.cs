namespace LeaveManagement.API.Models;

public static class UserRole
{
    public const string Employee = "Employee";
    public const string Manager = "Manager";

    public static bool IsValid(string? role)
    {
        return role is Employee or Manager;
    }
}
