using HRM.Razor.Models.ViewModels.AttendanceAdjustments;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.AttendanceAdjustments
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IAttendanceAdjustmentApiClient _adjustmentApiClient;

        public EditModel(IAttendanceAdjustmentApiClient adjustmentApiClient)
        {
            _adjustmentApiClient = adjustmentApiClient;
        }

        [BindProperty]
        public UpdateAttendanceAdjustmentViewModel Input { get; set; } = new();

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

            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không tìm thấy thông tin yêu cầu điều chỉnh.";
                return RedirectToPage("/AttendanceAdjustments/Index");
            }

            Adjustment = response.Data;

            if (Adjustment.Status != "PENDING")
            {
                TempData["ErrorMessage"] = "Chỉ các yêu cầu ở trạng thái Chờ duyệt (PENDING) mới có thể chỉnh sửa.";
                return RedirectToPage("/AttendanceAdjustments/Details", new { id });
            }

            Input = new UpdateAttendanceAdjustmentViewModel
            {
                AdjustmentId = Adjustment.AdjustmentId,
                RequestedCheckInTime = Adjustment.RequestedCheckInTime,
                RequestedCheckOutTime = Adjustment.RequestedCheckOutTime,
                Reason = Adjustment.Reason
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            ViewData["ActivePage"] = "AttendanceAdjustment";

            if (!ModelState.IsValid)
            {
                var fetchRes = await _adjustmentApiClient.GetAdjustmentByIdAsync(id);
                if (fetchRes.Success && fetchRes.Data != null)
                {
                    Adjustment = fetchRes.Data;
                }
                return Page();
            }

            var response = await _adjustmentApiClient.UpdateAdjustmentAsync(id, Input);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!response.Success)
            {
                ModelState.AddModelError(string.Empty, response.Message ?? "Cập nhật yêu cầu điều chỉnh thất bại.");
                var fetchRes = await _adjustmentApiClient.GetAdjustmentByIdAsync(id);
                if (fetchRes.Success && fetchRes.Data != null)
                {
                    Adjustment = fetchRes.Data;
                }
                return Page();
            }

            TempData["SuccessMessage"] = "Cập nhật yêu cầu điều chỉnh chấm công thành công!";
            return RedirectToPage("/AttendanceAdjustments/Index");
        }
    }
}
