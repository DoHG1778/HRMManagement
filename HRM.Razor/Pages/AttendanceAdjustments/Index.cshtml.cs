using HRM.Razor.Models.ViewModels.AttendanceAdjustments;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.AttendanceAdjustments
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IAttendanceAdjustmentApiClient _adjustmentApiClient;

        public IndexModel(IAttendanceAdjustmentApiClient adjustmentApiClient)
        {
            _adjustmentApiClient = adjustmentApiClient;
        }

        public List<AttendanceAdjustmentItemModel> Adjustments { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Month { get; set; } = DateTime.Today.Month;

        [BindProperty(SupportsGet = true)]
        public int? Year { get; set; } = DateTime.Today.Year;

        // Statistics
        public int TotalCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int CancelledCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "AttendanceAdjustment";

            var response = await _adjustmentApiClient.GetMyAdjustmentsAsync(Status, Month, Year);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.Success && response.Data != null)
            {
                Adjustments = response.Data;
                TotalCount = Adjustments.Count;
                PendingCount = Adjustments.Count(a => a.Status == "PENDING");
                ApprovedCount = Adjustments.Count(a => a.Status == "APPROVED");
                RejectedCount = Adjustments.Count(a => a.Status == "REJECTED");
                CancelledCount = Adjustments.Count(a => a.Status == "CANCELLED");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int id)
        {
            ViewData["ActivePage"] = "AttendanceAdjustment";

            if (id <= 0)
            {
                return RedirectToPage("/AttendanceAdjustments/Index");
            }

            var response = await _adjustmentApiClient.CancelAdjustmentAsync(id);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message ?? "Hủy yêu cầu điều chỉnh thất bại.";
            }
            else
            {
                TempData["SuccessMessage"] = "Đã hủy yêu cầu điều chỉnh thành công!";
            }

            return RedirectToPage("/AttendanceAdjustments/Index", new { status = Status, month = Month, year = Year });
        }
    }
}
