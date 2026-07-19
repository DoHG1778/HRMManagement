using System.Net.Http.Headers;
using HRM.Business.Common;
using HRM.Business.DTOs.Attendances;
using HRM.MVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class AttendanceManagementController : Controller
    {
        private const string TokenSessionKey = "AccessToken";
        private readonly IHttpClientFactory _httpClientFactory;

        public AttendanceManagementController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // View cá nhân cho Employee
        public async Task<IActionResult> Index(AttendanceFilterDto filter)
        {
            if (!HasRole("Employee")) return RedirectToAction("Index", "Home");

            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var today = DateOnly.FromDateTime(DateTime.Now);
            var historyResponse = await client.GetAsync($"api/attendances/me?fromDate={today}&toDate={today}");
            var historyData = await ApiResponseReader.ReadAsync<PagedResult<AttendanceResponseDto>>(historyResponse);
            
            ViewBag.TodayRecord = historyData?.Data?.Items?.FirstOrDefault();

            filter.PageNumber = filter.PageNumber == 0 ? 1 : filter.PageNumber;
            filter.PageSize = filter.PageSize == 0 ? 10 : filter.PageSize;

            var queryString = $"?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
            if (filter.FromDate.HasValue) queryString += $"&fromDate={filter.FromDate:yyyy-MM-dd}";
            if (filter.ToDate.HasValue) queryString += $"&toDate={filter.ToDate:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(filter.Status)) queryString += $"&status={filter.Status}";

            var listResponse = await client.GetAsync($"api/attendances/me{queryString}");
            var listData = await ApiResponseReader.ReadAsync<PagedResult<AttendanceResponseDto>>(listResponse);

            if (listResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Auth");

            ViewBag.Filter = filter;
            return View(listData?.Data ?? PagedResult<AttendanceResponseDto>.Create(new List<AttendanceResponseDto>(), filter.PageNumber, filter.PageSize, 0));
        }

        // View tổng quát cho HR và Manager
        public async Task<IActionResult> Sheet(AttendanceFilterDto filter)
        {
            if (!HasAnyRole("HR", "Manager")) return RedirectToAction("Index", "Home");

            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            filter.PageNumber = filter.PageNumber == 0 ? 1 : filter.PageNumber;
            filter.PageSize = filter.PageSize == 0 ? 10 : filter.PageSize;

            var queryString = $"?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
            if (filter.FromDate.HasValue) queryString += $"&fromDate={filter.FromDate:yyyy-MM-dd}";
            if (filter.ToDate.HasValue) queryString += $"&toDate={filter.ToDate:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(filter.Status)) queryString += $"&status={filter.Status}";
            if (!string.IsNullOrEmpty(filter.Keyword)) queryString += $"&keyword={Uri.EscapeDataString(filter.Keyword)}";

            var response = await client.GetAsync($"api/attendances{queryString}");
            var result = await ApiResponseReader.ReadAsync<PagedResult<AttendanceResponseDto>>(response);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized) return RedirectToAction("Login", "Auth");
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden) return RedirectToAction("Index", "Home");

            ViewBag.Filter = filter;
            return View(result?.Data ?? PagedResult<AttendanceResponseDto>.Create(new List<AttendanceResponseDto>(), filter.PageNumber, filter.PageSize, 0));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckIn()
        {
            if (!HasRole("Employee")) return Forbid();

            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var response = await client.PostAsync("api/attendances/check-in", null);
            var result = await ApiResponseReader.ReadAsync<CheckInResponseDto>(response);

            if (result?.Success == true)
                TempData["Success"] = result.Message;
            else
                TempData["Error"] = result?.Message ?? "Failed to check in.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckOut()
        {
            if (!HasRole("Employee")) return Forbid();

            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var response = await client.PostAsync("api/attendances/check-out", null);
            var result = await ApiResponseReader.ReadAsync<CheckOutResponseDto>(response);

            if (result?.Success == true)
                TempData["Success"] = result.Message;
            else
                TempData["Error"] = result?.Message ?? "Failed to check out.";

            return RedirectToAction(nameof(Index));
        }

        private bool HasRole(string roleName)
        {
            var roles = HttpContext.Session.GetString("Roles") ?? "";
            return roles.Split(',').Any(r => r.Trim().Equals(roleName, StringComparison.OrdinalIgnoreCase));
        }

        private bool HasAnyRole(params string[] roleNames)
        {
            var roles = HttpContext.Session.GetString("Roles") ?? "";
            var userRoles = roles.Split(',').Select(r => r.Trim());
            return roleNames.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
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