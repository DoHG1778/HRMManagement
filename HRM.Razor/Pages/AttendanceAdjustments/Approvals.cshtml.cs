using HRM.Razor.Models.ViewModels.AttendanceAdjustments;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.AttendanceAdjustments
{
    [Authorize(Roles = "Manager")]
    public class ApprovalsModel : PageModel
    {
        private readonly IAttendanceAdjustmentApiClient _adjustmentApiClient;

        public ApprovalsModel(IAttendanceAdjustmentApiClient adjustmentApiClient)
        {
            _adjustmentApiClient = adjustmentApiClient;
        }

        public List<AttendanceAdjustmentItemModel> PendingRequests { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? EmployeeId { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? Month { get; set; } = DateTime.Today.Month;

        [BindProperty(SupportsGet = true)]
        public int? Year { get; set; } = DateTime.Today.Year;

        // Statistics
        public int PendingCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "AttendanceAdjustmentApproval";

            var response = await _adjustmentApiClient.GetPendingAdjustmentsAsync(EmployeeId, Month, Year);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (response.Success && response.Data != null)
            {
                PendingRequests = response.Data;
                PendingCount = PendingRequests.Count;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(int id)
        {
            ViewData["ActivePage"] = "AttendanceAdjustmentApproval";

            if (id <= 0)
            {
                return RedirectToPage("/AttendanceAdjustments/Approvals");
            }

            var response = await _adjustmentApiClient.ApproveAdjustmentAsync(id);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message ?? "Phê duyệt yêu cầu điều chỉnh thất bại.";
            }
            else
            {
                TempData["SuccessMessage"] = "Phê duyệt yêu cầu điều chỉnh chấm công thành công!";
            }

            return RedirectToPage("/AttendanceAdjustments/Approvals", new { month = Month, year = Year, employeeId = EmployeeId });
        }

        public async Task<IActionResult> OnPostRejectAsync(int id, string rejectionReason)
        {
            ViewData["ActivePage"] = "AttendanceAdjustmentApproval";

            if (id <= 0)
            {
                return RedirectToPage("/AttendanceAdjustments/Approvals");
            }

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập lý do từ chối.";
                return RedirectToPage("/AttendanceAdjustments/Approvals", new { month = Month, year = Year, employeeId = EmployeeId });
            }

            var response = await _adjustmentApiClient.RejectAdjustmentAsync(id, rejectionReason);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message ?? "Từ chối yêu cầu điều chỉnh thất bại.";
            }
            else
            {
                TempData["SuccessMessage"] = "Đã từ chối yêu cầu điều chỉnh chấm công thành công!";
            }

            return RedirectToPage("/AttendanceAdjustments/Approvals", new { month = Month, year = Year, employeeId = EmployeeId });
        }
    }
}
