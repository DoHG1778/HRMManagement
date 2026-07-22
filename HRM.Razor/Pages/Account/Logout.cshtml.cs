using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Account
{
    [Authorize]
    [IgnoreAntiforgeryToken(Order = 1000)]
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            // 1. Clear JWT from Server-Side Session
            HttpContext.Session.Remove("JWToken");
            HttpContext.Session.Clear();

            // 2. Sign out Cookie Authentication
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            TempData["SuccessMessage"] = "Đã đăng xuất khỏi hệ thống thành công.";

            return RedirectToPage("/Account/Login");
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Index");
        }
    }
}
