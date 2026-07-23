using HRM.Business.DTOs.Leaves;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Leave
{
    public class ManagerPendingModel : PageModel
    {
        private readonly ILeaveApiClient _leaveApiClient;

        public ManagerPendingModel(ILeaveApiClient leaveApiClient)
        {
            _leaveApiClient = leaveApiClient;
        }

        public List<LeaveRequestResponseDto> LeaveRequests { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var response = await _leaveApiClient.GetPendingLeaveRequestsAsync();
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }
            if (response.StatusCode == 403)
                return RedirectToPage("/Account/AccessDenied");

            LeaveRequests = response.Data ?? new List<LeaveRequestResponseDto>();
            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id, bool isApproved, string? rejectionReason)
        {
            var response = await _leaveApiClient.ApproveLeaveRequestAsync(id, new ApproveLeaveRequestDto
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
                TempData["SuccessMessage"] = isApproved ? "Leave request approved." : "Leave request rejected.";
            else
                TempData["ErrorMessage"] = response.Message ?? "Failed to process.";

            return RedirectToPage();
        }
    }
}
