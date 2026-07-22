using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Profile
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IEmployeeApiClient _employeeApiClient;

        public IndexModel(IEmployeeApiClient employeeApiClient)
        {
            _employeeApiClient = employeeApiClient;
        }

        public MyProfileViewModel Data { get; set; } = new();

        [BindProperty]
        public UpdateMyProfileViewModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "Profile";

            var response = await _employeeApiClient.GetMyProfileAsync();

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!response.Success || response.Data == null)
            {
                Data.HasLinkedEmployee = false;
                return Page();
            }

            Data.HasLinkedEmployee = true;
            Data.Profile = response.Data;

            Input = new UpdateMyProfileViewModel
            {
                Phone = response.Data.Phone,
                Address = response.Data.Address,
                AvatarUrl = response.Data.AvatarUrl
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActivePage"] = "Profile";

            if (!ModelState.IsValid)
            {
                await ReloadProfileAsync();
                return Page();
            }

            var response = await _employeeApiClient.UpdateMyProfileAsync(Input);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!response.Success)
            {
                ModelState.AddModelError(string.Empty, response.Message ?? "Cập nhật hồ sơ cá nhân thất bại.");
                await ReloadProfileAsync();
                return Page();
            }

            TempData["SuccessMessage"] = "Cập nhật thành công thông tin hồ sơ cá nhân!";
            return RedirectToPage("/Profile/Index");
        }

        private async Task ReloadProfileAsync()
        {
            var response = await _employeeApiClient.GetMyProfileAsync();
            if (response.Success && response.Data != null)
            {
                Data.HasLinkedEmployee = true;
                Data.Profile = response.Data;
            }
            else
            {
                Data.HasLinkedEmployee = false;
            }
        }
    }
}
