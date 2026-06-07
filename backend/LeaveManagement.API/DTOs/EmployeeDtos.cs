using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.DTOs;

public class EmployeeDto
{
    public int EmployeeID { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class EmployeeCreateUpdateDto
{
    [Required, MaxLength(20)]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(100), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Department { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
