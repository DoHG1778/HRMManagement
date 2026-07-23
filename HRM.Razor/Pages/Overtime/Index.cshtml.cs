using HRM.Business.Common;
using HRM.Business.DTOs.Overtimes;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Overtime
{
    public class IndexModel : PageModel
    {
        private readonly IOvertimeApiClient _overtimeApiClient;

        public IndexModel(IOvertimeApiClient overtimeApiClient)
        {
            _overtimeApiClient = overtimeApiClient;
        }

        public List<OvertimeRequestResponseDto> OvertimeRequests { get; set; } = new();
        public string? CurrentStatus { get; set; }
        public int? CurrentMonth { get; set; }
        public int? CurrentYear { get; set; }

        public async Task<IActionResult> OnGetAsync(string? status, int? month, int? year)
        {
            CurrentStatus = status;
            CurrentMonth = month;
            CurrentYear = year;

            var response = await _overtimeApiClient.GetMyOvertimeRequestsAsync(month, year, status);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }
            if (response.StatusCode == 403)
                return RedirectToPage("/Account/AccessDenied");

            OvertimeRequests = response.Data?.Items ?? new List<OvertimeRequestResponseDto>();
            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            var response = await _overtimeApiClient.CancelOvertimeRequestAsync(id);
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success)
                TempData["SuccessMessage"] = "Overtime request cancelled.";
            else
                TempData["ErrorMessage"] = response.Message ?? "Failed to cancel.";

            return RedirectToPage();
        }
    }
}
