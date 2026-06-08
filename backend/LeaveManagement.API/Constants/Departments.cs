namespace LeaveManagement.API.Constants;

public static class Departments
{
    public static readonly string[] Values =
    [
        "IT",
        "HR",
        "Finance",
        "Marketing",
        "Sales",
        "Operations",
        "Engineering",
        "Customer Support"
    ];

    public static bool IsValid(string? department)
    {
        return Values.Contains(department?.Trim(), StringComparer.Ordinal);
    }
}
