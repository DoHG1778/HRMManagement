using System.Net.Http.Headers;
using System.Net.Http.Json;
using HRM.Business.Common;
using HRM.Business.DTOs.Departments;
using HRM.MVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class DepartmentManagementController : Controller
    {
        private const string TokenSessionKey = "AccessToken";
        private readonly IHttpClientFactory _httpClientFactory;

        public DepartmentManagementController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(bool? isActive)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var url = isActive.HasValue ? $"api/departments?isActive={isActive.Value}" : "api/departments";
            var httpResponse = await client.GetAsync(url);
            var response = await ApiResponseReader.ReadAsync<List<DepartmentResponseDto>>(httpResponse);
            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true)
                TempData["Error"] = response?.Message ?? "Failed to load departments.";

            return View(response?.Data ?? new List<DepartmentResponseDto>());
        }

        public IActionResult Create()
        {
            if (!HasToken()) return RedirectToAction("Login", "Auth");
            return View(new CreateDepartmentRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid) return View(model);

            var response = await client.PostAsJsonAsync("api/departments", model);
            var result = await ApiResponseReader.ReadAsync<DepartmentResponseDto>(response);
            var authFailure = RedirectOnAuthFailure(result);
            if (authFailure != null) return authFailure;

            if (result?.Success == true)
            {
                TempData["Success"] = "Department created.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to create department.");
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync($"api/departments/{id}");
            var response = await ApiResponseReader.ReadAsync<DepartmentResponseDto>(httpResponse);
            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true || response.Data == null)
                return RedirectToAction(nameof(Index));

            ViewBag.DepartmentId = id;
            return View(new UpdateDepartmentRequestDto
            {
                DepartmentName = response.Data.DepartmentName,
                Description = response.Data.Description,
                ManagerEmployeeId = response.Data.ManagerEmployeeId,
                IsActive = response.Data.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateDepartmentRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            ViewBag.DepartmentId = id;
            if (!ModelState.IsValid) return View(model);

            var response = await client.PutAsJsonAsync($"api/departments/{id}", model);
            var result = await ApiResponseReader.ReadAsync<DepartmentResponseDto>(response);
            var authFailure = RedirectOnAuthFailure(result);
            if (authFailure != null) return authFailure;

            if (result?.Success == true)
            {
                TempData["Success"] = "Department updated.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to update department.");
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
            var response = await client.PutAsJsonAsync($"api/departments/{id}/{action}", new { });
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
