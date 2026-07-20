using HRM.Business.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRM.API.Controllers
{
    [ApiController]
    [Route("api/dashboard")]
    [Authorize]
    public class DashboardController : BaseApiController
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary([FromQuery] int? month, [FromQuery] int? year)
        {
            var currentUser = GetCurrentUser();
            var result = await _dashboardService.GetDashboardSummaryAsync(currentUser, month, year);
            return HandleResponse(result);
        }

        [HttpGet("employee")]
        public async Task<IActionResult> GetEmployeeDashboard([FromQuery] int? month, [FromQuery] int? year)
        {
            var currentUser = GetCurrentUser();
            var result = await _dashboardService.GetEmployeeDashboardAsync(currentUser, month, year);
            return HandleResponse(result);
        }

        [HttpGet("manager")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> GetManagerDashboard([FromQuery] int? month, [FromQuery] int? year)
        {
            var currentUser = GetCurrentUser();
            var result = await _dashboardService.GetManagerDashboardAsync(currentUser, month, year);
            return HandleResponse(result);
        }

        [HttpGet("payroll")]
        [Authorize(Roles = "Admin,Payroll,Payroll Officer")]
        public async Task<IActionResult> GetPayrollDashboard([FromQuery] int? payrollMonth, [FromQuery] int? payrollYear)
        {
            var currentUser = GetCurrentUser();
            int month = payrollMonth ?? DateTime.Today.Month;
            int year = payrollYear ?? DateTime.Today.Year;
            var result = await _dashboardService.GetPayrollDashboardAsync(currentUser, month, year);
            return HandleResponse(result);
        }
    }
}