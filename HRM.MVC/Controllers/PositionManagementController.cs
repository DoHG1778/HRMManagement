using System.Net.Http.Headers;
using System.Net.Http.Json;
using HRM.Business.Common;
using HRM.Business.DTOs.Positions;
using HRM.MVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class PositionManagementController : Controller
    {
        private const string TokenSessionKey = "AccessToken";
        private readonly IHttpClientFactory _httpClientFactory;

        public PositionManagementController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(bool? isActive)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var url = isActive.HasValue ? $"api/positions?isActive={isActive.Value}" : "api/positions";
            var httpResponse = await client.GetAsync(url);
            var response = await ApiResponseReader.ReadAsync<List<PositionResponseDto>>(httpResponse);
            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true)
                TempData["Error"] = response?.Message ?? "Failed to load positions.";

            return View(response?.Data ?? new List<PositionResponseDto>());
        }

        public IActionResult Create()
        {
            if (!HasToken()) return RedirectToAction("Login", "Auth");
            return View(new CreatePositionRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePositionRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid) return View(model);

            var response = await client.PostAsJsonAsync("api/positions", model);
            var result = await ApiResponseReader.ReadAsync<PositionResponseDto>(response);
            var authFailure = RedirectOnAuthFailure(result);
            if (authFailure != null) return authFailure;

            if (result?.Success == true)
            {
                TempData["Success"] = "Position created.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to create position.");
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync($"api/positions/{id}");
            var response = await ApiResponseReader.ReadAsync<PositionResponseDto>(httpResponse);
            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true || response.Data == null)
                return RedirectToAction(nameof(Index));

            ViewBag.PositionId = id;
            return View(new UpdatePositionRequestDto
            {
                PositionName = response.Data.PositionName,
                Description = response.Data.Description,
                IsActive = response.Data.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdatePositionRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            ViewBag.PositionId = id;
            if (!ModelState.IsValid) return View(model);

            var response = await client.PutAsJsonAsync($"api/positions/{id}", model);
            var result = await ApiResponseReader.ReadAsync<PositionResponseDto>(response);
            var authFailure = RedirectOnAuthFailure(result);
            if (authFailure != null) return authFailure;

            if (result?.Success == true)
            {
                TempData["Success"] = "Position updated.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to update position.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            return await ToggleStatus(id, false);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            return await ToggleStatus(id, true);
        }

        private async Task<IActionResult> ToggleStatus(int id, bool activate)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var action = activate ? "activate" : "deactivate";
            var response = await client.PutAsJsonAsync($"api/positions/{id}/{action}", new { });
            var result = await ApiResponseReader.ReadAsync<bool>(response);
            var authFailure = RedirectOnAuthFailure(result);
            if (authFailure != null) return authFailure;

            TempData[result?.Success == true ? "Success" : "Error"] = result?.Message ?? "Status update failed.";
            return RedirectToAction(nameof(Index));
        }

        private IActionResult? RedirectOnAuthFailure<T>(ApiResponse<T>? response)
        {
            if (response?.StatusCode == 401)
            {
                HttpContext.Session.Clear();
                TempData["Error"] = "Your login session has expired. Please login again.";
                return RedirectToAction("Login", "Auth");
            }

            if (response?.StatusCode == 403)
            {
                TempData["Error"] = "You do not have permission to use this function.";
                return RedirectToAction("Index", "Home");
            }

            return null;
        }

        private bool HasToken()
        {
            return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(TokenSessionKey));
        }

        private HttpClient? CreateAuthorizedClient()
        {
            var token = HttpContext.Session.GetString(TokenSessionKey);
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var client = _httpClientFactory.CreateClient("HRM_API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
