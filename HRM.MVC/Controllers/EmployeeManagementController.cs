using System.Net.Http.Headers;
using System.Net.Http.Json;
using HRM.Business.Common;
using HRM.Business.DTOs.Departments;
using HRM.Business.DTOs.Employees;
using HRM.Business.DTOs.Positions;
using HRM.MVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class EmployeeManagementController : Controller
    {
        private const string TokenSessionKey = "AccessToken";
        private readonly IHttpClientFactory _httpClientFactory;

        public EmployeeManagementController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(EmployeeFilterDto filter)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            filter.PageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;

            var query = $"api/employees?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
            if (!string.IsNullOrWhiteSpace(filter.Keyword)) query += $"&keyword={Uri.EscapeDataString(filter.Keyword)}";
            if (filter.DepartmentId.HasValue) query += $"&departmentId={filter.DepartmentId}";
            if (filter.PositionId.HasValue) query += $"&positionId={filter.PositionId}";
            if (!string.IsNullOrWhiteSpace(filter.EmploymentStatus)) query += $"&employmentStatus={Uri.EscapeDataString(filter.EmploymentStatus)}";

            await LoadOrganizationLists(client);
            ViewBag.Filter = filter;

            var httpResponse = await client.GetAsync(query);
            var response = await ApiResponseReader.ReadAsync<PagedResult<EmployeeResponseDto>>(httpResponse);
            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true)
                TempData["Error"] = response?.Message ?? "Failed to load employees.";

            return View(response?.Data ?? PagedResult<EmployeeResponseDto>.Create(new List<EmployeeResponseDto>(), filter.PageNumber, filter.PageSize, 0));
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync($"api/employees/{id}");
            var response = await ApiResponseReader.ReadAsync<EmployeeDetailResponseDto>(httpResponse);
            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true || response.Data == null)
            {
                TempData["Error"] = response?.Message ?? "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        public IActionResult Create()
        {
            if (!HasToken()) return RedirectToAction("Login", "Auth");
            return View(new CreateEmployeeRequestDto { HireDate = DateOnly.FromDateTime(DateTime.Today) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateEmployeeRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid) return View(model);

            var response = await client.PostAsJsonAsync("api/employees", model);
            var result = await ApiResponseReader.ReadAsync<EmployeeResponseDto>(response);
            var authFailure = RedirectOnAuthFailure(result);
            if (authFailure != null) return authFailure;

            if (result?.Success == true)
            {
                TempData["Success"] = "Employee created.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to create employee.");
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync($"api/employees/{id}");
            var response = await ApiResponseReader.ReadAsync<EmployeeDetailResponseDto>(httpResponse);
            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true || response.Data == null)
            {
                TempData["Error"] = response?.Message ?? "Employee not found.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.EmployeeId = id;
            return View(new UpdateEmployeeRequestDto
            {
                FullName = response.Data.FullName,
                Gender = response.Data.Gender,
                DateOfBirth = response.Data.DateOfBirth,
                Phone = response.Data.Phone,
                Email = response.Data.Email,
                Address = response.Data.Address,
                Cccd = response.Data.Cccd,
                HireDate = response.Data.HireDate,
                EmploymentStatus = response.Data.EmploymentStatus,
                UserId = response.Data.UserId,
                ManagerId = response.Data.ManagerId,
                AvatarUrl = response.Data.AvatarUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateEmployeeRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            ViewBag.EmployeeId = id;
            if (!ModelState.IsValid) return View(model);

            var response = await client.PutAsJsonAsync($"api/employees/{id}", model);
            var result = await ApiResponseReader.ReadAsync<EmployeeResponseDto>(response);
            var authFailure = RedirectOnAuthFailure(result);
            if (authFailure != null) return authFailure;

            if (result?.Success == true)
            {
                TempData["Success"] = "Employee updated.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to update employee.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int id, string status)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var response = await client.PatchAsync($"api/employees/{id}/status?status={Uri.EscapeDataString(status)}", null);
            var result = await ApiResponseReader.ReadAsync<bool>(response);
            var authFailure = RedirectOnAuthFailure(result);
            if (authFailure != null) return authFailure;

            TempData[result?.Success == true ? "Success" : "Error"] = result?.Message ?? "Failed to change status.";
            return RedirectToAction(nameof(Details), new { id });
        }

        public async Task<IActionResult> Assign(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            await LoadOrganizationLists(client);
            ViewBag.EmployeeId = id;
            return View(new AssignEmployeeRequestDto { StartDate = DateOnly.FromDateTime(DateTime.Today) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Assign(int id, AssignEmployeeRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            ViewBag.EmployeeId = id;
            await LoadOrganizationLists(client);
            if (!ModelState.IsValid) return View(model);

            var response = await client.PostAsJsonAsync($"api/employees/{id}/assignment", model);
            var result = await ApiResponseReader.ReadAsync<EmployeeAssignmentResponseDto>(response);
            var authFailure = RedirectOnAuthFailure(result);
            if (authFailure != null) return authFailure;

            if (result?.Success == true)
            {
                TempData["Success"] = "Employee assigned.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to assign employee.");
            return View(model);
        }

        public async Task<IActionResult> Transfer(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            await LoadOrganizationLists(client);
            ViewBag.EmployeeId = id;
            return View(new TransferEmployeeRequestDto { StartDate = DateOnly.FromDateTime(DateTime.Today) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(int id, TransferEmployeeRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            ViewBag.EmployeeId = id;
            await LoadOrganizationLists(client);
            if (!ModelState.IsValid) return View(model);

            var response = await client.PostAsJsonAsync($"api/employees/{id}/transfer", model);
            var result = await ApiResponseReader.ReadAsync<EmployeeAssignmentResponseDto>(response);
            var authFailure = RedirectOnAuthFailure(result);
            if (authFailure != null) return authFailure;

            if (result?.Success == true)
            {
                TempData["Success"] = "Employee transferred.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to transfer employee.");
            return View(model);
        }

        public async Task<IActionResult> MyProfile()
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync("api/employees/me");
            var response = await ApiResponseReader.ReadAsync<EmployeeDetailResponseDto>(httpResponse);
            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true || response.Data == null)
            {
                TempData["Error"] = response?.Message ?? "Failed to load profile.";
                return RedirectToAction("Index", "Home");
            }

            return View(response.Data);
        }

        public async Task<IActionResult> EditMyProfile()
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync("api/employees/me");
            var response = await ApiResponseReader.ReadAsync<EmployeeDetailResponseDto>(httpResponse);
            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true || response.Data == null)
                return RedirectToAction(nameof(MyProfile));

            return View(new UpdateMyProfileRequestDto
            {
                Phone = response.Data.Phone,
                Address = response.Data.Address,
                AvatarUrl = response.Data.AvatarUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMyProfile(UpdateMyProfileRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid) return View(model);

            var response = await client.PutAsJsonAsync("api/employees/me", model);
            var result = await ApiResponseReader.ReadAsync<EmployeeDetailResponseDto>(response);
            var authFailure = RedirectOnAuthFailure(result);
            if (authFailure != null) return authFailure;

            if (result?.Success == true)
            {
                TempData["Success"] = "Profile updated.";
                return RedirectToAction(nameof(MyProfile));
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to update profile.");
            return View(model);
        }

        private async Task LoadOrganizationLists(HttpClient client)
        {
            var departmentsResponse = await client.GetAsync("api/departments?isActive=true");
            var positionsResponse = await client.GetAsync("api/positions?isActive=true");
            var departments = await ApiResponseReader.ReadAsync<List<DepartmentResponseDto>>(departmentsResponse);
            var positions = await ApiResponseReader.ReadAsync<List<PositionResponseDto>>(positionsResponse);

            if (departments.StatusCode == 401 || positions.StatusCode == 401)
            {
                HttpContext.Session.Clear();
                TempData["Error"] = "Your login session has expired. Please login again.";
            }
            else if (departments.StatusCode == 403 || positions.StatusCode == 403)
            {
                TempData["Error"] = "You do not have permission to load organization data.";
            }

            ViewBag.Departments = departments?.Data ?? new List<DepartmentResponseDto>();
            ViewBag.Positions = positions?.Data ?? new List<PositionResponseDto>();
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
