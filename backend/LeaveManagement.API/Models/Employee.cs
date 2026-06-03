using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagement.API.Models;

public class Employee
{
    [Key]
    public int EmployeeID { get; set; }

    [Required, MaxLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string EmployeeCode { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(100), EmailAddress]
    [Column(TypeName = "varchar(100)")]
    public string Email { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string Department { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
