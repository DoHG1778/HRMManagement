using HRM.Business.Common;
using HRM.Business.DTOs.Leaves;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Leave
{
    public class IndexModel : PageModel
    {
        private readonly ILeaveApiClient _leaveApiClient;

        public IndexModel(ILeaveApiClient leaveApiClient)
        {
            _leaveApiClient = leaveApiClient;
        }

        public List<LeaveRequestResponseDto> LeaveRequests { get; set; } = new();
        public string? CurrentStatus { get; set; }
        public int? CurrentMonth { get; set; }
        public int? CurrentYear { get; set; }

        public async Task<IActionResult> OnGetAsync(string? status, int? month, int? year)
        {
            CurrentStatus = status;
            CurrentMonth = month;
            CurrentYear = year;

            var response = await _leaveApiClient.GetMyLeaveRequestsAsync(month, year, status);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }
            if (response.StatusCode == 403)
                return RedirectToPage("/Account/AccessDenied");

            LeaveRequests = response.Data?.Items ?? new List<LeaveRequestResponseDto>();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var response = await _leaveApiClient.CancelLeaveRequestAsync(id);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                TempData["SuccessMessage"] = "Leave request cancelled.";
            else
                TempData["ErrorMessage"] = response.Message ?? "Failed to cancel.";

            return RedirectToPage();
        }
    }
}
