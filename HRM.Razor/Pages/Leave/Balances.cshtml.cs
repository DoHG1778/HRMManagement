using HRM.Business.DTOs.Leaves;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Leave
{
    public class BalancesModel : PageModel
    {
        private readonly ILeaveApiClient _leaveApiClient;

        public BalancesModel(ILeaveApiClient leaveApiClient)
        {
            _leaveApiClient = leaveApiClient;
        }

        public List<LeaveBalanceResponseDto> Balances { get; set; } = new();
        public int CurrentEmployeeId { get; set; } = 1;
        public int CurrentYear { get; set; } = DateTime.Now.Year;

        public async Task<IActionResult> OnGetAsync(int? employeeId, int? year)
        {
            CurrentEmployeeId = employeeId ?? 1;
            CurrentYear = year ?? DateTime.Now.Year;

            var response = await _leaveApiClient.GetLeaveBalancesAsync(CurrentEmployeeId, CurrentYear);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }
            if (response.StatusCode == 403)
                return RedirectToPage("/Account/AccessDenied");

            Balances = response.Data ?? new List<LeaveBalanceResponseDto>();
            return Page();
        }
    }
}
