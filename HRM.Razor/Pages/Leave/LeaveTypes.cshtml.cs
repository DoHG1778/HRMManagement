using HRM.Business.DTOs.Leaves;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Leave
{
    public class LeaveTypesModel : PageModel
    {
        private readonly ILeaveApiClient _leaveApiClient;

        public LeaveTypesModel(ILeaveApiClient leaveApiClient)
        {
            _leaveApiClient = leaveApiClient;
        }

        public List<LeaveTypeResponseDto> LeaveTypes { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var response = await _leaveApiClient.GetLeaveTypesAsync();
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }
            if (response.StatusCode == 403)
                return RedirectToPage("/Account/AccessDenied");

            LeaveTypes = response.Data ?? new List<LeaveTypeResponseDto>();
            return Page();
        }

        public async Task<IActionResult> OnPostDeactivateAsync(int id)
        {
            var response = await _leaveApiClient.DeactivateLeaveTypeAsync(id);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                TempData["SuccessMessage"] = "Leave type deactivated.";
            else
                TempData["ErrorMessage"] = response.Message ?? "Failed to deactivate.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostActivateAsync(int id)
        {
            var response = await _leaveApiClient.ActivateLeaveTypeAsync(id);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                TempData["SuccessMessage"] = "Leave type activated.";
            else
                TempData["ErrorMessage"] = response.Message ?? "Failed to activate.";

            return RedirectToPage();
        }
    }
}
