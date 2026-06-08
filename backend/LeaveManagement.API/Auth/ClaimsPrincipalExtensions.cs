using System.Security.Claims;

namespace LeaveManagement.API.Auth;

public static class ClaimsPrincipalExtensions
{
    public static int GetEmployeeId(this ClaimsPrincipal user)
    {
        var employeeId = user.FindFirstValue("employeeId");
        return int.TryParse(employeeId, out var id) ? id : 0;
    }
}
