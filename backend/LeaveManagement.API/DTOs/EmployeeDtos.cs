using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.DTOs;

public class EmployeeCreateUpdateDto
{
    [Required, MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Department { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = "Employee";

    [MinLength(8)]
    public string? Password { get; set; }

    public bool IsActive { get; set; } = true;

    public int? ManagerID { get; set; }
}
