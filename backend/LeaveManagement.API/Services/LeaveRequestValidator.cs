using LeaveManagement.API.Data;
using LeaveManagement.API.DTOs;
using LeaveManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LeaveManagement.API.Services;

public class LeaveRequestValidator(LeaveManagementDbContext context)
{
    public async Task<List<string>> ValidateCreateAsync(LeaveRequestCreateDto dto, string status = LeaveStatus.Pending)
    {
        var errors = ValidateDatesAndReason(dto.FromDate, dto.ToDate, dto.Reason);

        if (!LeaveStatus.IsValid(status))
        {
            errors.Add("Status must be Pending, Approved, or Rejected.");
        }

        if (!await context.Employees.AnyAsync(e => e.EmployeeID == dto.EmployeeID))
        {
            errors.Add("EmployeeID does not exist.");
        }

        if (!await context.LeaveTypes.AnyAsync(lt => lt.LeaveTypeID == dto.LeaveTypeID))
        {
            errors.Add("LeaveTypeID does not exist.");
        }

        return errors;
    }

    public static List<string> ValidateDatesAndReason(DateOnly fromDate, DateOnly toDate, string? reason)
    {
        var errors = new List<string>();
        var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

        if (toDate < fromDate)
        {
            errors.Add("ToDate must be greater than or equal to FromDate.");
        }

        if (fromDate < today || toDate < today)
        {
            errors.Add("Leave dates cannot be in the past.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            errors.Add("Reason is mandatory.");
        }

        return errors;
    }

    public static int CalculateNumberOfDays(DateOnly fromDate, DateOnly toDate)
    {
        return toDate.DayNumber - fromDate.DayNumber + 1;
    }
}
