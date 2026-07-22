using HRM.Razor.Models.ViewModels.AttendanceAdjustments;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRM.Razor.Pages.AttendanceAdjustments
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IAttendanceAdjustmentApiClient _adjustmentApiClient;

        public CreateModel(IAttendanceAdjustmentApiClient adjustmentApiClient)
        {
            _adjustmentApiClient = adjustmentApiClient;
        }

        [BindProperty]
        public CreateAttendanceAdjustmentViewModel Input { get; set; } = new();

        public List<AdjustableAttendanceModel> AdjustableAttendances { get; set; } = new();
        public List<SelectListItem> AttendanceOptions { get; set; } = new();
        public AdjustableAttendanceModel? SelectedAttendance { get; set; }

        public async Task<IActionResult> OnGetAsync(int? attendanceId, int? month, int? year)
        {
            ViewData["ActivePage"] = "AttendanceAdjustment";

            await LoadPageDataAsync(attendanceId, month, year);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActivePage"] = "AttendanceAdjustment";

            if (!ModelState.IsValid)
            {
                await LoadPageDataAsync(Input.AttendanceId, null, null);
                return Page();
            }

            var response = await _adjustmentApiClient.CreateAdjustmentAsync(Input);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!response.Success)
            {
                ModelState.AddModelError(string.Empty, response.Message ?? "Tạo yêu cầu điều chỉnh thất bại.");
                await LoadPageDataAsync(Input.AttendanceId, null, null);
                return Page();
            }

            TempData["SuccessMessage"] = "Gửi yêu cầu điều chỉnh chấm công thành công!";
            return RedirectToPage("/AttendanceAdjustments/Index");
        }

        private async Task LoadPageDataAsync(int? attendanceId, int? month, int? year)
        {
            var res = await _adjustmentApiClient.GetAdjustableAttendancesAsync(month, year);
            if (res.Success && res.Data != null)
            {
                AdjustableAttendances = res.Data;
                AttendanceOptions = AdjustableAttendances
                    .Select(a => new SelectListItem
                    {
                        Value = a.AttendanceId.ToString(),
                        Text = $"{a.AttendanceDate:dd/MM/yyyy} (Vào: {(a.CheckInTime.HasValue ? a.CheckInTime.Value.ToString("HH:mm") : "--:--")}, Ra: {(a.CheckOutTime.HasValue ? a.CheckOutTime.Value.ToString("HH:mm") : "--:--")})"
                    })
                    .ToList();
            }

            if (attendanceId.HasValue && attendanceId.Value > 0)
            {
                Input.AttendanceId = attendanceId.Value;
                SelectedAttendance = AdjustableAttendances.FirstOrDefault(a => a.AttendanceId == attendanceId.Value);

                if (SelectedAttendance != null && !Input.RequestedCheckInTime.HasValue)
                {
                    Input.RequestedCheckInTime = SelectedAttendance.CheckInTime;
                    Input.RequestedCheckOutTime = SelectedAttendance.CheckOutTime;
                }
            }
            else if (AdjustableAttendances.Any())
            {
                Input.AttendanceId = AdjustableAttendances.First().AttendanceId;
                SelectedAttendance = AdjustableAttendances.First();

                if (!Input.RequestedCheckInTime.HasValue)
                {
                    Input.RequestedCheckInTime = SelectedAttendance.CheckInTime;
                    Input.RequestedCheckOutTime = SelectedAttendance.CheckOutTime;
                }
            }

            AttendanceOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn ngày chấm công cần điều chỉnh --" });
        }
    }
}
