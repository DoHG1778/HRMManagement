using HRM.Razor.Models.ViewModels.AttendanceAdjustments;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.AttendanceAdjustments
{
    [Authorize]
    public class DetailsModel : PageModel
    {
        private readonly IAttendanceAdjustmentApiClient _adjustmentApiClient;

        public DetailsModel(IAttendanceAdjustmentApiClient adjustmentApiClient)
        {
            _adjustmentApiClient = adjustmentApiClient;
        }

        public AttendanceAdjustmentItemModel Adjustment { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ViewData["ActivePage"] = "AttendanceAdjustment";

            if (id <= 0)
            {
                return RedirectToPage("/AttendanceAdjustments/Index");
            }

            var response = await _adjustmentApiClient.GetAdjustmentByIdAsync(id);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không tìm thấy thông tin yêu cầu điều chỉnh.";
                return RedirectToPage("/AttendanceAdjustments/Index");
            }

            Adjustment = response.Data;
            return Page();
        }
    }
}
