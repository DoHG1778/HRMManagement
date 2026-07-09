using System.Net.Http.Json;
using HRM.Business.Common;
using HRM.Business.DTOs.Overtimes;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class StaffOvertimeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public StaffOvertimeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(int? month, int? year, string? status)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var query = $"api/overtimes/requests/me?pageNumber=1&pageSize=50";
            if (!string.IsNullOrEmpty(status)) query += $"&status={status}";
            if (month.HasValue) query += $"&month={month}";
            if (year.HasValue) query += $"&year={year}";

            var response = await client.GetFromJsonAsync<ApiResponse<PagedResult<OvertimeRequestResponseDto>>>(query);
            var requests = response?.Data?.Items ?? new List<OvertimeRequestResponseDto>();

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentMonth = month;
            ViewBag.CurrentYear = year;
            return View(requests);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateOvertimeRequestDto model)
        {
            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient("HRM_API");
                var response = await client.PostAsJsonAsync("api/overtimes/requests", model);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<OvertimeRequestResponseDto>>();

                if (result?.Success == true)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError("", result?.Message ?? "Failed to create overtime request.");
            }
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.GetFromJsonAsync<ApiResponse<PagedResult<OvertimeRequestResponseDto>>>("api/overtimes/requests/me?pageNumber=1&pageSize=100");
            var request = response?.Data?.Items.FirstOrDefault(r => r.Otid == id);
            if (request == null)
                return NotFound();

            if (request.Status != "PENDING")
            {
                TempData["Error"] = "Only PENDING requests can be edited.";
                return RedirectToAction(nameof(Index));
            }

            return View(new UpdateOvertimeRequestDto
            {
                Otdate = request.Otdate,
                StartTime = request.StartTime,
                EndTime = request.EndTime,
                Reason = request.Reason ?? string.Empty
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateOvertimeRequestDto model)
        {
            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient("HRM_API");
                var response = await client.PutAsJsonAsync($"api/overtimes/requests/{id}", model);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<OvertimeRequestResponseDto>>();

                if (result?.Success == true)
                    return RedirectToAction(nameof(Index));

                ModelState.AddModelError("", result?.Message ?? "Failed to update overtime request.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.PutAsJsonAsync($"api/overtimes/requests/{id}/cancel", new { });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();

            if (result?.Success == true)
                TempData["Success"] = "Overtime request cancelled.";
            else
                TempData["Error"] = result?.Message ?? "Failed to cancel.";
            return RedirectToAction(nameof(Index));
        }
    }
}
