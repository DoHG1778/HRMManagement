using HRM.Business.Common;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HRM.API.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected CurrentUser GetCurrentUser()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var employeeIdClaim = User.FindFirst("EmployeeId")?.Value;

            int.TryParse(userIdClaim, out int userId);
            int.TryParse(employeeIdClaim, out int employeeId);

            var roles = User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return new CurrentUser
            {
                UserId = userId,
                EmployeeId = employeeId > 0 ? employeeId : null,
                Username = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                Roles = roles
            };
        }

        protected IActionResult HandleResponse<T>(ApiResponse<T> response)
        {
            if (response.StatusCode <= 0)
            {
                response.StatusCode = response.Success ? 200 : 400;
            }

            return StatusCode(response.StatusCode, response);
        }
    }
}