using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.Models;

public class AppUser
{
    public int AppUserID { get; set; }

    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(128)]
    public string PasswordHash { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Role { get; set; } = string.Empty;

    public int EmployeeID { get; set; }

    public Employee Employee { get; set; } = null!;
}
