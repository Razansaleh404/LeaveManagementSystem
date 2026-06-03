using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagement.API.Models;

public class LeaveRequest
{
    [Key]
    public int RequestID { get; set; }

    public int EmployeeID { get; set; }
    public int LeaveTypeID { get; set; }

    [Column(TypeName = "date")]
    public DateTime FromDate { get; set; }

    [Column(TypeName = "date")]
    public DateTime ToDate { get; set; }

    public int NumberOfDays { get; set; }

    [Required, MaxLength(500)]
    [Column(TypeName = "varchar(500)")]
    public string Reason { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    [Column(TypeName = "varchar(20)")]
    public string Status { get; set; } = LeaveRequestStatus.Pending;

    [Column(TypeName = "datetime")]
    public DateTime CreatedDate { get; set; }

    [MaxLength(500)]
    [Column(TypeName = "varchar(500)")]
    public string? ManagerComments { get; set; }

    public Employee? Employee { get; set; }
    public LeaveType? LeaveType { get; set; }
}
