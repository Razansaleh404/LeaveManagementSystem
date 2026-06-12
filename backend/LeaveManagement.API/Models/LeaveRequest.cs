using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;      // Required, MaxLength, IValidatableObject
using System.ComponentModel.DataAnnotations.Schema; // Column
using LeaveManagement.API.Models;

public class LeaveRequest : IValidatableObject
{
    public int RequestID { get; set; }

    [Required]
    public int EmployeeID { get; set; }

    [Required]
    public int LeaveTypeID { get; set; }

    [Required]
    public int ManagerID { get; set; }

    [Column(TypeName = "date")]
    public DateOnly FromDate { get; set; }

    [Column(TypeName = "date")]
    public DateOnly ToDate { get; set; }

    public int NumberOfDays { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Status { get; set; } = LeaveStatus.Pending;

    [MaxLength(500)]
    public string? ManagerComments { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;

    public LeaveType LeaveType { get; set; } = null!;

    [JsonIgnore]
    public Employee AssignedManager { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ToDate < FromDate)
            yield return new ValidationResult("ToDate must be greater than or equal to FromDate.", new[] { nameof(ToDate) });

        if (FromDate < DateOnly.FromDateTime(DateTime.UtcNow.Date) || ToDate < DateOnly.FromDateTime(DateTime.UtcNow.Date))
            yield return new ValidationResult("Leave dates cannot be in the past.", new[] { nameof(FromDate), nameof(ToDate) });

        if (!LeaveStatus.IsValid(Status))
            yield return new ValidationResult("Status must be Pending, Approved, or Rejected.", new[] { nameof(Status) });
    }
}