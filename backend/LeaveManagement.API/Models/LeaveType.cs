using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LeaveManagement.API.Models;

public class LeaveType
{
    public int LeaveTypeID { get; set; }

    [Required, MaxLength(50)]
    public string LeaveName { get; set; } = string.Empty;

    [JsonIgnore]
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = [];
}