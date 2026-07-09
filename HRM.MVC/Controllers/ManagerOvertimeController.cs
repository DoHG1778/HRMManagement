using System.Net.Http.Json;
using HRM.Business.Common;
using HRM.Business.DTOs.Overtimes;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class ManagerOvertimeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ManagerOvertimeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.GetFromJsonAsync<ApiResponse<List<OvertimeRequestResponseDto>>>("api/overtimes/requests/pending");
            var requests = response?.Data ?? new List<OvertimeRequestResponseDto>();
            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, bool isApproved, string? rejectionReason)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.PutAsJsonAsync($"api/overtimes/requests/{id}/approval", new { isApproved, rejectionReason });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<OvertimeRequestResponseDto>>();

            if (result?.Success == true)
                TempData["Success"] = isApproved ? "Overtime request approved." : "Overtime request rejected.";
            else
                TempData["Error"] = result?.Message ?? "Failed to process.";
            return RedirectToAction(nameof(Index));
        }
    }
}
