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
            var config = HttpContext.RequestServices.GetService(typeof(Microsoft.Extensions.Configuration.IConfiguration)) as Microsoft.Extensions.Configuration.IConfiguration;
            var env = HttpContext.RequestServices.GetService(typeof(Microsoft.AspNetCore.Hosting.IWebHostEnvironment)) as Microsoft.AspNetCore.Hosting.IWebHostEnvironment;
            bool enableMockUser = false;
            if (env != null && env.EnvironmentName.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                enableMockUser = config?.GetValue<bool>("EnableMockUser") ?? false;
            }

            if (!User.Identity?.IsAuthenticated ?? true)
            {
                if (enableMockUser)
                {
                    return new CurrentUser
                    {
                        UserId = 1,
                        EmployeeId = 1,
                        Username = "admin",
                        Email = "admin@hrm.com",
                        Roles = new List<string> { "Admin", "HR", "Manager", "Employee", "Payroll", "Payroll Officer" }
                    };
                }
                return new CurrentUser();
            }

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