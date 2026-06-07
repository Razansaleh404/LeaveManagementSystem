namespace LeaveManagement.API.Models;

public static class LeaveStatus
{
    public const string Pending = "Pending";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";

    public static readonly string[] All = [Pending, Approved, Rejected];

    public static bool IsValid(string? status) => !string.IsNullOrWhiteSpace(status) && All.Contains(status);
}
