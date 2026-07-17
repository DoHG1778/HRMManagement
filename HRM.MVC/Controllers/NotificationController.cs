using System.Net.Http.Headers;
using HRM.Business.Common;
using HRM.Business.DTOs.Notifications;
using HRM.MVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class NotificationController : Controller
    {
        private const string TokenSessionKey = "AccessToken";
        private readonly IHttpClientFactory _httpClientFactory;

        public NotificationController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync("api/notifications/me");

            var response =
                await ApiResponseReader.ReadAsync<PagedResult<NotificationResponseDto>>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null)
                return authFailure;

            if (response?.Success != true)
            {
                TempData["Error"] = response?.Message ?? "Failed to load notifications.";

                return View(
                    PagedResult<NotificationResponseDto>.Create(
                        new List<NotificationResponseDto>(),
                        1,
                        10,
                        0));
            }

            return View(response.Data);
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var httpResponse =
                await client.GetAsync($"api/notifications/{id}");

            var response =
                await ApiResponseReader.ReadAsync<NotificationResponseDto>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null)
                return authFailure;

            if (response?.Success != true || response.Data == null)
            {
                TempData["Error"] =
                    response?.Message ?? "Notification not found.";

                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var httpResponse =
                await client.PutAsync($"api/notifications/{id}/read", null);

            var response =
                await ApiResponseReader.ReadAsync<bool>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null)
                return authFailure;

            TempData[response?.Success == true ? "Success" : "Error"] =
                response?.Message ?? "Failed.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var httpResponse =
                await client.PutAsync($"api/notifications/{id}/delete", null);

            var response =
                await ApiResponseReader.ReadAsync<bool>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null)
                return authFailure;

            TempData[response?.Success == true ? "Success" : "Error"] =
                response?.Message ?? "Failed.";

            return RedirectToAction(nameof(Index));
        }

        private IActionResult? RedirectOnAuthFailure<T>(ApiResponse<T>? response)
        {
            if (response?.StatusCode == 401)
            {
                HttpContext.Session.Clear();

                TempData["Error"] =
                    "Your login session has expired. Please login again.";

                return RedirectToAction("Login", "Auth");
            }

            if (response?.StatusCode == 403)
            {
                TempData["Error"] =
                    "You do not have permission to use this function.";

                return RedirectToAction("Index", "Home");
            }

            return null;
        }

        private HttpClient? CreateAuthorizedClient()
        {
            var token = HttpContext.Session.GetString(TokenSessionKey);

            if (string.IsNullOrWhiteSpace(token))
                return null;

            var client = _httpClientFactory.CreateClient("HRM_API");

            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            return client;
        }
    }
}