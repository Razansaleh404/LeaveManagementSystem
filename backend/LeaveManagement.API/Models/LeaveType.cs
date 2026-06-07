using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.Models;

public class LeaveType
{
    public int LeaveTypeID { get; set; }

    [Required, MaxLength(50)]
    public string LeaveName { get; set; } = string.Empty;

    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
}
