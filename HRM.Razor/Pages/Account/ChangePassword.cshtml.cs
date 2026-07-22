using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Account
{
    [Authorize]
    public class ChangePasswordModel : PageModel
    {
        private readonly IAuthApiClient _authApiClient;

        public ChangePasswordModel(IAuthApiClient authApiClient)
        {
            _authApiClient = authApiClient;
        }

        [BindProperty]
        public ChangePasswordViewModel Input { get; set; } = new();

        public void OnGet()
        {
            ViewData["ActivePage"] = "ChangePassword";
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActivePage"] = "ChangePassword";

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. Gọi API Đổi mật khẩu
            var response = await _authApiClient.ChangePasswordAsync(Input);

            // 2. Xử lý Token hết hạn hoặc 401
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                HttpContext.Session.Clear();
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToPage("/Account/Login");
            }

            // 3. Xử lý thất bại khác
            if (!response.Success)
            {
                var errorMessage = string.IsNullOrWhiteSpace(response.Message)
                    ? "Đổi mật khẩu thất bại. Vui lòng kiểm tra lại thông tin."
                    : response.Message;

                ModelState.AddModelError(string.Empty, errorMessage);
                return Page();
            }

            // 4. Thành công -> Xóa dữ liệu mật khẩu khỏi form & thông báo
            TempData["SuccessMessage"] = "Mật khẩu của bạn đã được thay đổi thành công!";
            Input = new ChangePasswordViewModel();
            ModelState.Clear();

            return Page();
        }
    }
}
