using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.DTOs;

public class LeaveRequestDto
{
    public int RequestID { get; set; }
    public int EmployeeID { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int LeaveTypeID { get; set; }
    public string LeaveTypeName { get; set; } = string.Empty;
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int NumberOfDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public string? ManagerComments { get; set; }
}

public class LeaveRequestCreateDto
{
    [Required]
    public int EmployeeID { get; set; }

    [Required]
    public int LeaveTypeID { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public class LeaveDecisionDto
{
    [MaxLength(500)]
    public string? ManagerComments { get; set; }
}
