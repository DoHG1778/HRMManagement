using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using HRM.Business.Common;
using HRM.Business.DTOs.Attendances;
using HRM.Business.DTOs.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class AttendanceAdjustmentController : Controller
    {
        private const string TokenSessionKey = "AccessToken";
        private readonly IHttpClientFactory _httpClientFactory;

        public AttendanceAdjustmentController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        // GET: AttendanceAdjustment/MyRequests
        public async Task<IActionResult> MyRequests(int? month, int? year, string? status)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var query = "api/attendance-adjustments/my?";
            if (!string.IsNullOrEmpty(status)) query += $"status={status}&";
            if (month.HasValue) query += $"month={month.Value}&";
            if (year.HasValue) query += $"year={year.Value}&";

            var response = await client.GetAsync(query);
            var result = await ReadApiResponseAsync<List<AttendanceAdjustmentResponseDto>>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null) return authResult;

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentMonth = month;
            ViewBag.CurrentYear = year;

            var list = result?.Data ?? new List<AttendanceAdjustmentResponseDto>();
            return View(list);
        }

        // GET: AttendanceAdjustment/Create
        public async Task<IActionResult> Create(int? month, int? year)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var query = "api/attendance-adjustments/available-attendances?";
            if (month.HasValue) query += $"month={month.Value}&";
            if (year.HasValue) query += $"year={year.Value}&";

            var response = await client.GetAsync(query);
            var result = await ReadApiResponseAsync<List<AdjustableAttendanceDto>>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null) return authResult;

            ViewBag.AvailableAttendances = result?.Data ?? new List<AdjustableAttendanceDto>();
            ViewBag.CurrentMonth = month;
            ViewBag.CurrentYear = year;

            return View(new CreateAttendanceAdjustmentRequestDto());
        }

        // POST: AttendanceAdjustment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAttendanceAdjustmentRequestDto model, int? month, int? year)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                var query = "api/attendance-adjustments/available-attendances?";
                if (month.HasValue) query += $"month={month.Value}&";
                if (year.HasValue) query += $"year={year.Value}&";

                var response = await client.GetAsync(query);
                var result = await ReadApiResponseAsync<List<AdjustableAttendanceDto>>(response);
                ViewBag.AvailableAttendances = result?.Data ?? new List<AdjustableAttendanceDto>();
                return View(model);
            }

            var postResponse = await client.PostAsJsonAsync("api/attendance-adjustments", model);
            var postResult = await ReadApiResponseAsync<AttendanceAdjustmentResponseDto>(postResponse);

            var authResult = RedirectOnAuthFailure(postResult);
            if (authResult != null) return authResult;

            if (postResult?.Success == true)
            {
                TempData["Success"] = "Adjustment request created successfully.";
                return RedirectToAction(nameof(MyRequests));
            }

            ModelState.AddModelError("", postResult?.Message ?? "Failed to create adjustment request.");

            // Reload dropdown
            var reloadQuery = "api/attendance-adjustments/available-attendances?";
            if (month.HasValue) reloadQuery += $"month={month.Value}&";
            if (year.HasValue) reloadQuery += $"year={year.Value}&";
            var response2 = await client.GetAsync(reloadQuery);
            var result2 = await ReadApiResponseAsync<List<AdjustableAttendanceDto>>(response2);
            ViewBag.AvailableAttendances = result2?.Data ?? new List<AdjustableAttendanceDto>();

            return View(model);
        }

        // GET: AttendanceAdjustment/Edit/{id}
        public async Task<IActionResult> Edit(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.GetAsync($"api/attendance-adjustments/{id}");
            var result = await ReadApiResponseAsync<AttendanceAdjustmentResponseDto>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null) return authResult;

            if (result?.Success != true || result.Data == null)
            {
                TempData["Error"] = result?.Message ?? "Adjustment request not found.";
                return RedirectToAction(nameof(MyRequests));
            }

            if (result.Data.Status != "PENDING")
            {
                TempData["Error"] = "Only PENDING requests can be edited.";
                return RedirectToAction(nameof(MyRequests));
            }

            ViewBag.AdjustmentDetail = result.Data;

            return View(new UpdateAttendanceAdjustmentRequestDto
            {
                RequestedCheckInTime = result.Data.RequestedCheckInTime,
                RequestedCheckOutTime = result.Data.RequestedCheckOutTime,
                Reason = result.Data.Reason
            });
        }

        // POST: AttendanceAdjustment/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateAttendanceAdjustmentRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                var response = await client.GetAsync($"api/attendance-adjustments/{id}");
                var result = await ReadApiResponseAsync<AttendanceAdjustmentResponseDto>(response);
                ViewBag.AdjustmentDetail = result?.Data;
                return View(model);
            }

            var putResponse = await client.PutAsJsonAsync($"api/attendance-adjustments/{id}", model);
            var putResult = await ReadApiResponseAsync<AttendanceAdjustmentResponseDto>(putResponse);

            var authResult = RedirectOnAuthFailure(putResult);
            if (authResult != null) return authResult;

            if (putResult?.Success == true)
            {
                TempData["Success"] = "Adjustment request updated successfully.";
                return RedirectToAction(nameof(MyRequests));
            }

            ModelState.AddModelError("", putResult?.Message ?? "Failed to update adjustment request.");
            
            var responseReload = await client.GetAsync($"api/attendance-adjustments/{id}");
            var resultReload = await ReadApiResponseAsync<AttendanceAdjustmentResponseDto>(responseReload);
            ViewBag.AdjustmentDetail = resultReload?.Data;

            return View(model);
        }

        // POST: AttendanceAdjustment/Cancel/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.PutAsJsonAsync($"api/attendance-adjustments/{id}/cancel", new { });
            var result = await ReadApiResponseAsync<bool>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null) return authResult;

            if (result?.Success == true)
                TempData["Success"] = "Adjustment request cancelled successfully.";
            else
                TempData["Error"] = result?.Message ?? "Failed to cancel request.";

            return RedirectToAction(nameof(MyRequests));
        }

        // GET: AttendanceAdjustment/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.GetAsync($"api/attendance-adjustments/{id}");
            var result = await ReadApiResponseAsync<AttendanceAdjustmentResponseDto>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null) return authResult;

            if (result?.Success != true || result.Data == null)
            {
                TempData["Error"] = result?.Message ?? "Adjustment request not found.";
                return RedirectToAction(nameof(MyRequests));
            }

            var meResponse = await client.GetAsync("api/auth/me");
            var meResult = await ReadApiResponseAsync<UserResponseDto>(meResponse);
            if (meResult?.Success == true && meResult.Data != null)
            {
                ViewBag.CurrentEmployeeId = meResult.Data.EmployeeId;
            }
            else
            {
                ViewBag.CurrentEmployeeId = null;
            }

            return View(result.Data);
        }

        // GET: AttendanceAdjustment/Pending
        public async Task<IActionResult> Pending(int? month, int? year, int? employeeId)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var query = "api/attendance-adjustments/pending?";
            if (employeeId.HasValue) query += $"employeeId={employeeId.Value}&";
            if (month.HasValue) query += $"month={month.Value}&";
            if (year.HasValue) query += $"year={year.Value}&";

            var response = await client.GetAsync(query);
            var result = await ReadApiResponseAsync<List<AttendanceAdjustmentResponseDto>>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null) return authResult;

            // Load employee list for filter
            var empResponse = await client.GetAsync("api/employees?pageNumber=1&pageSize=1000");
            try
            {
                var content = await empResponse.Content.ReadAsStringAsync();
                var empResult = JsonSerializer.Deserialize<ApiResponse<PagedResult<HRM.Business.DTOs.Employees.EmployeeResponseDto>>>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                ViewBag.EmployeesList = empResult?.Data?.Items;
            }
            catch
            {
                ViewBag.EmployeesList = null;
            }

            ViewBag.CurrentEmployeeId = employeeId;
            ViewBag.CurrentMonth = month;
            ViewBag.CurrentYear = year;

            var list = result?.Data ?? new List<AttendanceAdjustmentResponseDto>();
            return View(list);
        }

        // POST: AttendanceAdjustment/Approve/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            var response = await client.PutAsJsonAsync($"api/attendance-adjustments/{id}/approve", new { });
            var result = await ReadApiResponseAsync<AttendanceAdjustmentResponseDto>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null) return authResult;

            if (result?.Success == true)
                TempData["Success"] = "Attendance adjustment request approved.";
            else
                TempData["Error"] = result?.Message ?? "Failed to approve request.";

            return RedirectToAction(nameof(Pending));
        }

        // POST: AttendanceAdjustment/Reject/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string rejectionReason)
        {
            var client = CreateAuthorizedClient();
            if (client == null)
                return RedirectToAction("Login", "Auth");

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["Error"] = "Rejection reason is required.";
                return RedirectToAction(nameof(Pending));
            }

            var model = new RejectAttendanceAdjustmentRequestDto { RejectionReason = rejectionReason };
            var response = await client.PutAsJsonAsync($"api/attendance-adjustments/{id}/reject", model);
            var result = await ReadApiResponseAsync<AttendanceAdjustmentResponseDto>(response);

            var authResult = RedirectOnAuthFailure(result);
            if (authResult != null) return authResult;

            if (result?.Success == true)
                TempData["Success"] = "Attendance adjustment request rejected.";
            else
                TempData["Error"] = result?.Message ?? "Failed to reject request.";

            return RedirectToAction(nameof(Pending));
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

        private static async Task<ApiResponse<T>> ReadApiResponseAsync<T>(HttpResponseMessage response)
        {
            var content = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrWhiteSpace(content))
            {
                try
                {
                    var res = JsonSerializer.Deserialize<ApiResponse<T>>(content, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    if (res != null)
                    {
                        return res;
                    }
                }
                catch
                {
                    // Fall through
                }
            }

            return ApiResponse<T>.Fail("Unable to process the request. Please try again.", (int)response.StatusCode);
        }
    }
}
