using LeaveManagement.API.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeaveManagement.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DepartmentsController : ControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<string>> GetDepartments()
    {
        return Departments.Values;
    }
}
