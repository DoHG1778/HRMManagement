using System.Security.Claims;
using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IAuthApiClient _authApiClient;

        public LoginModel(IAuthApiClient authApiClient)
        {
            _authApiClient = authApiClient;
        }

        [BindProperty]
        public LoginViewModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public void OnGet(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                Response.Redirect(Url.Content("~/"));
                return;
            }

            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // 1. Trim input và gọi API đăng nhập
            Input.UsernameOrEmail = Input.UsernameOrEmail?.Trim() ?? string.Empty;
            var response = await _authApiClient.LoginAsync(Input);

            // 2. Xử lý thất bại
            if (!response.Success || response.Data == null)
            {
                Input.Password = string.Empty; // Xóa mật khẩu khi lỗi

                var errorMessage = response.StatusCode switch
                {
                    401 => "Tên đăng nhập/email hoặc mật khẩu không chính xác.",
                    403 => response.Message ?? "Tài khoản bị khóa hoặc không có quyền truy cập.",
                    404 => "Không tìm thấy thông tin tài khoản.",
                    500 => "Lỗi hệ thống phía API. Vui lòng thử lại sau.",
                    _ => string.IsNullOrWhiteSpace(response.Message) ? "Đăng nhập thất bại." : response.Message
                };

                ModelState.AddModelError(string.Empty, errorMessage);
                return Page();
            }

            var userData = response.Data;

            // 3. Lưu JWT Token phía Server-side Session (Không lưu LocalStorage/JS)
            HttpContext.Session.SetString("JWToken", userData.Token);

            // 4. Tạo Claims Identity & Claims Principal
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userData.UserId.ToString()),
                new Claim(ClaimTypes.Name, userData.Username),
                new Claim(ClaimTypes.Email, userData.Email ?? string.Empty),
                new Claim("UserId", userData.UserId.ToString())
            };

            if (userData.EmployeeId.HasValue)
            {
                claims.Add(new Claim("EmployeeId", userData.EmployeeId.Value.ToString()));
            }

            if (userData.Roles != null && userData.Roles.Any())
            {
                foreach (var role in userData.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "Employee"));
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = Input.RememberMe,
                ExpiresUtc = userData.ExpiredAt != default ? userData.ExpiredAt : DateTimeOffset.UtcNow.AddHours(8),
                AllowRefresh = true
            };

            // 5. Thực hiện Sign-In Cookie Authentication
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            TempData["SuccessMessage"] = $"Chào mừng {userData.Username} đã đăng nhập thành công!";

            // 6. Redirect an toàn
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl != "/")
            {
                return Redirect(returnUrl);
            }

            return RedirectToRoleDashboard(userData.Roles);
        }

        private IActionResult RedirectToRoleDashboard(List<string>? roles)
        {
            // Kiểm tra role ưu tiên theo thứ tự cao xuống thấp
            if (roles != null && roles.Any())
            {
                if (roles.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase) || r.Equals("System Administrator", StringComparison.OrdinalIgnoreCase)))
                {
                    return RedirectToPage("/Index");
                }
                if (roles.Any(r => r.Equals("HR", StringComparison.OrdinalIgnoreCase) || r.Equals("HR Staff", StringComparison.OrdinalIgnoreCase)))
                {
                    return RedirectToPage("/Index");
                }
                if (roles.Any(r => r.Equals("Payroll", StringComparison.OrdinalIgnoreCase) || r.Equals("Payroll Officer", StringComparison.OrdinalIgnoreCase)))
                {
                    return RedirectToPage("/Index");
                }
                if (roles.Any(r => r.Equals("Manager", StringComparison.OrdinalIgnoreCase) || r.Equals("Department Manager", StringComparison.OrdinalIgnoreCase)))
                {
                    return RedirectToPage("/Index");
                }
            }

            return RedirectToPage("/Index");
        }
    }
}
