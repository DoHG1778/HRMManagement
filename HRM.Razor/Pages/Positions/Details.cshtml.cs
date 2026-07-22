using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Positions
{
    [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator")]
    public class DetailsModel : PageModel
    {
        private readonly IPositionApiClient _positionApiClient;

        public DetailsModel(IPositionApiClient positionApiClient)
        {
            _positionApiClient = positionApiClient;
        }

        public PositionModel Position { get; set; } = new();

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
                TempData["ErrorMessage"] = "Không tìm thấy thông tin chức vụ yêu cầu.";
                return RedirectToPage("/Positions/Index");
            }

            Position = response.Data;
            return Page();
        }
    }
}
