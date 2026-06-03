using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LeaveManagement.API.Models;

public class LeaveType
{
    [Key]
    public int LeaveTypeID { get; set; }

    [Required, MaxLength(50)]
    [Column(TypeName = "varchar(50)")]
    public string LeaveName { get; set; } = string.Empty;

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}
