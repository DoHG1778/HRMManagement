using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Positions
{
    [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
    public class EditModel : PageModel
    {
        private readonly IPositionApiClient _positionApiClient;

        public EditModel(IPositionApiClient positionApiClient)
        {
            _positionApiClient = positionApiClient;
        }

        [BindProperty]
        public EditPositionViewModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ViewData["ActivePage"] = "Positions";

            if (id <= 0)
            {
                return RedirectToPage("/Positions/Index");
            }

            var response = await _positionApiClient.GetPositionByIdAsync(id);

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
                TempData["ErrorMessage"] = "Không tìm thấy chức vụ yêu cầu.";
                return RedirectToPage("/Positions/Index");
            }

            var pos = response.Data;
            Input = new EditPositionViewModel
            {
                PositionId = pos.PositionId,
                PositionName = pos.PositionName,
                Description = pos.Description,
                IsActive = pos.IsActive
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            ViewData["ActivePage"] = "Positions";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Input.PositionName = Input.PositionName?.Trim() ?? string.Empty;

            var response = await _positionApiClient.UpdatePositionAsync(id, Input);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!response.Success)
            {
                ModelState.AddModelError(string.Empty, response.Message ?? "Cập nhật chức vụ thất bại.");
                return Page();
            }

            TempData["SuccessMessage"] = $"Đã cập nhật thông tin chức vụ {Input.PositionName}!";
            return RedirectToPage("/Positions/Index");
        }
    }
}
