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

            return View(response?.Data ?? PagedResult<UserResponseDto>.Create(new List<UserResponseDto>(), pageNumber, pageSize, 0));
        }

        public async Task<IActionResult> Create()
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            // Load Employees without User accounts to link
            var empResponse = await client.GetAsync("api/employees?pageSize=100");
            var employees = await ApiResponseReader.ReadAsync<PagedResult<EmployeeResponseDto>>(empResponse);
            
            // Load Roles
            var roleResponse = await client.GetAsync("api/users/roles");
            var roles = await ApiResponseReader.ReadAsync<List<RoleResponseDto>>(roleResponse);

            ViewBag.Employees = employees?.Data?.Items?.Where(e => e.UserId == null).ToList() ?? new List<EmployeeResponseDto>();
            ViewBag.Roles = roles?.Data ?? new List<RoleResponseDto>();

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
                await ReloadViewData(client);
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
            await ReloadViewData(client);
            return View(model);
        }

        private async Task ReloadViewData(HttpClient client)
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