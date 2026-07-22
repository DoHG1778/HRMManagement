using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Positions
{
    [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
    public class CreateModel : PageModel
    {
        private readonly IPositionApiClient _positionApiClient;

        public CreateModel(IPositionApiClient positionApiClient)
        {
            _positionApiClient = positionApiClient;
        }

        [BindProperty]
        public CreatePositionViewModel Input { get; set; } = new();

        public void OnGet()
        {
            ViewData["ActivePage"] = "Positions";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActivePage"] = "Positions";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            Input.PositionName = Input.PositionName?.Trim() ?? string.Empty;

            var response = await _positionApiClient.CreatePositionAsync(Input);

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
                ModelState.AddModelError(string.Empty, response.Message ?? "Tạo chức vụ thất bại.");
                return Page();
            }

            TempData["SuccessMessage"] = $"Đã tạo thành công chức vụ {Input.PositionName}!";
            return RedirectToPage("/Positions/Index");
        }
    }
}
