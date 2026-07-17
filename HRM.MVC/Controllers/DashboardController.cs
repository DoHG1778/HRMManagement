using System.Net.Http.Headers;
using System.Net.Http.Json;
using HRM.Business.Common;
using HRM.Business.DTOs.Dashboards;
using HRM.MVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class DashboardController : Controller
    {
        private const string TokenSessionKey = "AccessToken";
        private readonly IHttpClientFactory _httpClientFactory;

        public DashboardController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Summary(int? month, int? year)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.GetAsync(
                $"api/dashboard/summary?month={month}&year={year}");

            var result =
                await ApiResponseReader.ReadAsync<DashboardSummaryDto>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null)
                return authResult;

            if (result?.Success != true)
            {
                TempData["Error"] = result?.Message ?? "Cannot load dashboard.";
                return View(new DashboardSummaryDto());
            }

            return View(result.Data);
        }

        public async Task<IActionResult> Employee(int? month, int? year)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.GetAsync(
                $"api/dashboard/employee?month={month}&year={year}");

            var result =
                await ApiResponseReader.ReadAsync<EmployeeDashboardDto>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null)
                return authResult;

            if (result?.Success != true)
            {
                TempData["Error"] = result?.Message ?? "Cannot load dashboard.";
                return View(new EmployeeDashboardDto());
            }

            return View(result.Data);
        }

        public async Task<IActionResult> Manager(int? month, int? year)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.GetAsync(
                $"api/dashboard/manager?month={month}&year={year}");

            var result =
                await ApiResponseReader.ReadAsync<ManagerDashboardDto>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null)
                return authResult;

            if (result?.Success != true)
            {
                TempData["Error"] = result?.Message ?? "Cannot load dashboard.";
                return View(new ManagerDashboardDto());
            }

            return View(result.Data);
        }

        public async Task<IActionResult> Payroll(int payrollMonth, int payrollYear)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.GetAsync(
                $"api/dashboard/payroll?payrollMonth={payrollMonth}&payrollYear={payrollYear}");

            var result =
                await ApiResponseReader.ReadAsync<PayrollDashboardDto>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null)
                return authResult;

            if (result?.Success != true)
            {
                TempData["Error"] = result?.Message ?? "Cannot load dashboard.";
                return View(new PayrollDashboardDto());
            }

            return View(result.Data);
        }

        private IActionResult? RedirectOnAuthFailure<T>(ApiResponse<T>? response)
        {
            if (response?.StatusCode == 401)
            {
                HttpContext.Session.Clear();
                TempData["Error"] = "Your login session has expired.";
                return RedirectToAction("Login", "Auth");
            }

            if (response?.StatusCode == 403)
            {
                TempData["Error"] = "You do not have permission.";
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