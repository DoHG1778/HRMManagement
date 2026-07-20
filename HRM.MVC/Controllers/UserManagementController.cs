using System.Net.Http.Headers;
using System.Net.Http.Json;
using HRM.Business.Common;
using HRM.Business.DTOs.Employees;
using HRM.Business.DTOs.Users;
using HRM.MVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class UserManagementController : Controller
    {
        private const string TokenSessionKey = "AccessToken";
        private readonly IHttpClientFactory _httpClientFactory;

        public UserManagementController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(string? keyword, bool? isActive, int pageNumber = 1, int pageSize = 10)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var query = $"api/users?pageNumber={pageNumber}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(keyword)) query += $"&keyword={Uri.EscapeDataString(keyword)}";
            if (isActive.HasValue) query += $"&isActive={isActive.Value}";

            var httpResponse = await client.GetAsync(query);
            var response = await ApiResponseReader.ReadAsync<PagedResult<UserResponseDto>>(httpResponse);
            
            if (response?.StatusCode == 401) return RedirectToAction("Login", "Auth");
            if (response?.StatusCode == 403) return RedirectToAction("Index", "Home");

            ViewBag.Keyword = keyword;
            ViewBag.IsActive = isActive;

            return View(response?.Data ?? PagedResult<UserResponseDto>.Create(new List<UserResponseDto>(), pageNumber, pageSize, 0));
        }

        public async Task<IActionResult> Create()
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            await LoadViewData(client);
            return View(new CreateUserRequestDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                await LoadViewData(client);
                return View(model);
            }

            var response = await client.PostAsJsonAsync("api/users", model);
            var result = await ApiResponseReader.ReadAsync<UserResponseDto>(response);

            if (result?.Success == true)
            {
                TempData["Success"] = "User account created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to create user account.");
            await LoadViewData(client);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync($"api/users/{id}");
            var response = await ApiResponseReader.ReadAsync<UserResponseDto>(httpResponse);
            
            if (response?.Success != true)
            {
                TempData["Error"] = response?.Message ?? "User not found.";
                return RedirectToAction(nameof(Index));
            }

            await LoadViewData(client);
            
            // Get current Role IDs
            var allRoles = ViewBag.Roles as List<RoleResponseDto> ?? new List<RoleResponseDto>();
            var userRoleIds = allRoles
                .Where(r => response.Data.Roles.Contains(r.RoleName))
                .Select(r => r.RoleId)
                .ToList();

            var model = new UpdateUserRequestDto
            {
                Email = response.Data.Email,
                IsActive = response.Data.IsActive,
                RoleIds = userRoleIds
            };

            ViewBag.Username = response.Data.Username;
            ViewBag.UserId = id;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateUserRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                await LoadViewData(client);
                return View(model);
            }

            var response = await client.PutAsJsonAsync($"api/users/{id}", model);
            var result = await ApiResponseReader.ReadAsync<UserResponseDto>(response);

            if (result?.Success == true)
            {
                TempData["Success"] = "User account updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to update user account.");
            await LoadViewData(client);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id, bool lockIt)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var action = lockIt ? "lock" : "unlock";
            var response = await client.PutAsync($"api/users/{id}/{action}", null);
            var result = await ApiResponseReader.ReadAsync<bool>(response);

            if (result?.Success == true)
                TempData["Success"] = $"User {(lockIt ? "locked" : "unlocked")} successfully.";
            else
                TempData["Error"] = result?.Message ?? $"Failed to {action} user.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadViewData(HttpClient client)
        {
            var empResponse = await client.GetAsync("api/employees?pageSize=100");
            var employees = await ApiResponseReader.ReadAsync<PagedResult<EmployeeResponseDto>>(empResponse);
            var roleResponse = await client.GetAsync("api/users/roles");
            var roles = await ApiResponseReader.ReadAsync<List<RoleResponseDto>>(roleResponse);

            ViewBag.Employees = employees?.Data?.Items?.Where(e => e.UserId == null).ToList() ?? new List<EmployeeResponseDto>();
            ViewBag.Roles = roles?.Data ?? new List<RoleResponseDto>();
        }

        private HttpClient? CreateAuthorizedClient()
        {
            var token = HttpContext.Session.GetString(TokenSessionKey);
            if (string.IsNullOrWhiteSpace(token)) return null;

            var client = _httpClientFactory.CreateClient("HRM_API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}