using System.ComponentModel.DataAnnotations;

namespace LeaveManagement.API.DTOs;

public class LeaveRequestCreateDto
{
    [Required]
    public int EmployeeID { get; set; }

    [Required]
    public int LeaveTypeID { get; set; }

    [Required]
    public DateOnly FromDate { get; set; }

    [Required]
    public DateOnly ToDate { get; set; }

    [Required(AllowEmptyStrings = false), MinLength(1), MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public class LeaveRequestUpdateDto : LeaveRequestCreateDto
{
    [Required, MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(500)]
    public string? ManagerComments { get; set; }
}

public class ManagerDecisionDto
{
    [MaxLength(500)]
    public string? ManagerComments { get; set; }
}
