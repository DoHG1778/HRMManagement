using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using HRM.Business.Common;
using HRM.Business.DTOs.Auth;
using HRM.Business.DTOs.Users;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class AuthController : Controller
    {
        private const string TokenSessionKey = "AccessToken";
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public IActionResult Login()
        {
            return View(new LoginRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginRequestDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.PostAsJsonAsync("api/auth/login", model);
            var result = await ReadApiResponseAsync<LoginResponseDto>(response);

            if (result?.Success == true && result.Data != null)
            {
                HttpContext.Session.SetString(TokenSessionKey, result.Data.Token);
                HttpContext.Session.SetString("Username", result.Data.Username);
                HttpContext.Session.SetString("Roles", string.Join(", ", result.Data.Roles));
                TempData["Success"] = "Login successfully.";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", result?.Message ?? "Login failed.");
            return View(model);
        }

        public IActionResult ChangePassword()
        {
            if (!HasToken())
                return RedirectToAction(nameof(Login));

            return View(new ChangePasswordRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequestDto model)
        {
            if (!HasToken())
                return RedirectToAction(nameof(Login));

            if (!ModelState.IsValid)
                return View(model);

            var client = CreateAuthorizedClient();
            var response = await client.PostAsJsonAsync("api/auth/change-password", model);
            var result = await ReadApiResponseAsync<bool>(response);

            if (result?.Success == true)
            {
                TempData["Success"] = "Password changed successfully.";
                return RedirectToAction(nameof(MyAccount));
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to change password.");
            return View(model);
        }

        public async Task<IActionResult> MyAccount()
        {
            if (!HasToken())
                return RedirectToAction(nameof(Login));

            var client = CreateAuthorizedClient();
            var httpResponse = await client.GetAsync("api/auth/me");
            var response = await ReadApiResponseAsync<UserResponseDto>(httpResponse);

            if (response?.Success == true && response.Data != null)
                return View(response.Data);

            TempData["Error"] = response?.Message ?? "Failed to load account.";
            return RedirectToAction(nameof(Login));
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Success"] = "Logged out.";
            return RedirectToAction(nameof(Login));
        }

        private bool HasToken()
        {
            return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(TokenSessionKey));
        }

        private HttpClient CreateAuthorizedClient()
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var token = HttpContext.Session.GetString(TokenSessionKey);
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return client;
        }

        private static async Task<ApiResponse<T>?> ReadApiResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return ApiResponse<T>.Fail($"Request failed with status {(int)response.StatusCode}.", (int)response.StatusCode);

            try
            {
                return JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                return ApiResponse<T>.Fail(
                    response.IsSuccessStatusCode ? "Invalid response from API." : $"API error: {content}",
                    (int)response.StatusCode);
            }
        }
    }
}
