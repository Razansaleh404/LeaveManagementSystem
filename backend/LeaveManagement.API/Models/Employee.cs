using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LeaveManagement.API.Models;

public class Employee
{
    public int EmployeeID { get; set; }

    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Department { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = UserRole.Employee;

    [Required]
    [JsonIgnore]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    [JsonIgnore]
    public string PasswordSalt { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int? ManagerID { get; set; }

    [JsonIgnore]
    public Employee? Manager { get; set; }

    [JsonIgnore]
    public ICollection<Employee> DirectReports { get; set; } = new List<Employee>();

    [JsonIgnore]
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();

    [JsonIgnore]
    public ICollection<LeaveRequest> AssignedLeaveRequests { get; set; } = new List<LeaveRequest>();
}
