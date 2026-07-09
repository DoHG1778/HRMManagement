using System.Net.Http.Json;
using System.Text.Json;
using HRM.Business.Common;
using HRM.Business.DTOs.Leaves;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class StaffLeaveController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StaffLeaveController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(int? month, int? year, string? status)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var query = $"api/leaves/requests/me?pageNumber=1&pageSize=50";
            if (!string.IsNullOrEmpty(status)) query += $"&status={status}";
            if (month.HasValue) query += $"&month={month}";
            if (year.HasValue) query += $"&year={year}";

            var response = await client.GetFromJsonAsync<ApiResponse<PagedResult<LeaveRequestResponseDto>>>(query);
            var requests = response?.Data?.Items ?? new List<LeaveRequestResponseDto>();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentMonth = month;
            ViewBag.CurrentYear = year;
            return View(requests);
        }

        public async Task<IActionResult> Create()
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var typesResponse = await client.GetFromJsonAsync<ApiResponse<List<LeaveTypeResponseDto>>>("api/leaves/types?isActive=true");
            ViewBag.LeaveTypes = typesResponse?.Data ?? new List<LeaveTypeResponseDto>();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLeaveRequestDto model)
        {
            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient("HRM_API");
                var response = await client.PostAsJsonAsync("api/leaves/requests", model);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeaveRequestResponseDto>>();

                if (result?.Success == true)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError("", result?.Message ?? "Failed to create leave request.");
            }

            var client2 = _httpClientFactory.CreateClient("HRM_API");
            var typesResponse = await client2.GetFromJsonAsync<ApiResponse<List<LeaveTypeResponseDto>>>("api/leaves/types?isActive=true");
            ViewBag.LeaveTypes = typesResponse?.Data ?? new List<LeaveTypeResponseDto>();
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.GetFromJsonAsync<ApiResponse<PagedResult<LeaveRequestResponseDto>>>("api/leaves/requests/me?pageNumber=1&pageSize=100");
            var request = response?.Data?.Items.FirstOrDefault(r => r.LeaveRequestId == id);
            if (request == null)
                return NotFound();

            if (request.Status != "PENDING")
            {
                TempData["Error"] = "Only PENDING requests can be edited.";
                return RedirectToAction(nameof(Index));
            }

            var typesResponse = await client.GetFromJsonAsync<ApiResponse<List<LeaveTypeResponseDto>>>("api/leaves/types?isActive=true");
            ViewBag.LeaveTypes = typesResponse?.Data ?? new List<LeaveTypeResponseDto>();

            var model = new UpdateLeaveRequestDto
            {
                LeaveTypeId = request.LeaveTypeId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Reason = request.Reason
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateLeaveRequestDto model)
        {
            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient("HRM_API");
                var response = await client.PutAsJsonAsync($"api/leaves/requests/{id}", model);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeaveRequestResponseDto>>();

                if (result?.Success == true)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError("", result?.Message ?? "Failed to update leave request.");
            }

            var client2 = _httpClientFactory.CreateClient("HRM_API");
            var typesResponse = await client2.GetFromJsonAsync<ApiResponse<List<LeaveTypeResponseDto>>>("api/leaves/types?isActive=true");
            ViewBag.LeaveTypes = typesResponse?.Data ?? new List<LeaveTypeResponseDto>();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.PutAsJsonAsync($"api/leaves/requests/{id}/cancel", new { });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();

            if (result?.Success == true)
                TempData["Success"] = "Leave request cancelled.";
            else
                TempData["Error"] = result?.Message ?? "Failed to cancel.";
            return RedirectToAction(nameof(Index));
        }
    }
}
