using HRM.Business.DTOs.Overtimes;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Overtime
{
    public class ManagerPendingModel : PageModel
    {
        private readonly IOvertimeApiClient _overtimeApiClient;

        public ManagerPendingModel(IOvertimeApiClient overtimeApiClient)
        {
            _overtimeApiClient = overtimeApiClient;
        }

        public List<OvertimeRequestResponseDto> OvertimeRequests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var response = await _overtimeApiClient.GetPendingOvertimeRequestsAsync();
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }
            if (response.StatusCode == 403)
                return RedirectToPage("/Account/AccessDenied");

            OvertimeRequests = response.Data ?? new List<OvertimeRequestResponseDto>();
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id, bool isApproved, string? rejectionReason)
        {
            var response = await _overtimeApiClient.ApproveOvertimeRequestAsync(id, new ApproveOvertimeRequestDto
            {
                IsApproved = isApproved,
                RejectionReason = rejectionReason
            });
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                TempData["SuccessMessage"] = isApproved ? "Overtime request approved." : "Overtime request rejected.";
            else
                TempData["ErrorMessage"] = response.Message ?? "Failed to process.";

            return RedirectToPage();
        }
    }
}
