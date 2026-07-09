using System.Net.Http.Json;
using HRM.Business.Common;
using HRM.Business.DTOs.Leaves;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class ManagerLeaveController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public ManagerLeaveController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index()
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.GetFromJsonAsync<ApiResponse<List<LeaveRequestResponseDto>>>("api/leaves/requests/pending");
            var requests = response?.Data ?? new List<LeaveRequestResponseDto>();
            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, bool isApproved, string? rejectionReason)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.PutAsJsonAsync($"api/leaves/requests/{id}/approval", new { isApproved, rejectionReason });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeaveRequestResponseDto>>();

            if (result?.Success == true)
                TempData["Success"] = isApproved ? "Leave request approved." : "Leave request rejected.";
            else
                TempData["Error"] = result?.Message ?? "Failed to process.";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> LeaveTypes()
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.GetFromJsonAsync<ApiResponse<List<LeaveTypeResponseDto>>>("api/leaves/types");
            var types = response?.Data ?? new List<LeaveTypeResponseDto>();
            return View(types);
        }

        public IActionResult CreateLeaveType()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLeaveType(CreateLeaveTypeRequestDto model)
        {
            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient("HRM_API");
                var response = await client.PostAsJsonAsync("api/leaves/types", model);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeaveTypeResponseDto>>();

                if (result?.Success == true)
                    return RedirectToAction(nameof(LeaveTypes));

                ModelState.AddModelError("", result?.Message ?? "Failed to create leave type.");
            }
            return View(model);
        }

        public async Task<IActionResult> EditLeaveType(int id)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.GetFromJsonAsync<ApiResponse<List<LeaveTypeResponseDto>>>("api/leaves/types");
            var type = response?.Data?.FirstOrDefault(t => t.LeaveTypeId == id);
            if (type == null) return NotFound();

            return View(new UpdateLeaveTypeRequestDto
            {
                LeaveTypeName = type.LeaveTypeName,
                MaxDaysPerYear = type.MaxDaysPerYear,
                Description = type.Description,
                IsActive = type.IsActive
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLeaveType(int id, UpdateLeaveTypeRequestDto model)
        {
            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient("HRM_API");
                var response = await client.PutAsJsonAsync($"api/leaves/types/{id}", model);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeaveTypeResponseDto>>();

                if (result?.Success == true)
                    return RedirectToAction(nameof(LeaveTypes));

                ModelState.AddModelError("", result?.Message ?? "Failed to update leave type.");
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateLeaveType(int id)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.PutAsJsonAsync($"api/leaves/types/{id}/deactivate", new { });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();

            if (result?.Success == true)
                TempData["Success"] = "Leave type deactivated.";
            else
                TempData["Error"] = result?.Message ?? "Failed to deactivate.";
            return RedirectToAction(nameof(LeaveTypes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateLeaveType(int id)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var response = await client.PutAsJsonAsync($"api/leaves/types/{id}/activate", new { });
            var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();

            if (result?.Success == true)
                TempData["Success"] = "Leave type activated.";
            else
                TempData["Error"] = result?.Message ?? "Failed to activate.";
            return RedirectToAction(nameof(LeaveTypes));
        }

        public async Task<IActionResult> Balances(int? employeeId, int? year)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var eid = employeeId ?? 1;
            var y = year ?? DateTime.Now.Year;
            var response = await client.GetFromJsonAsync<ApiResponse<List<LeaveBalanceResponseDto>>>($"api/leaves/balances/employee/{eid}?year={y}");
            ViewBag.Balances = response?.Data ?? new List<LeaveBalanceResponseDto>();
            ViewBag.EmployeeId = eid;
            ViewBag.Year = y;
            return View();
        }

        public async Task<IActionResult> SetBalance(int employeeId, int leaveTypeId, int year)
        {
            var client = _httpClientFactory.CreateClient("HRM_API");
            var typesResponse = await client.GetFromJsonAsync<ApiResponse<List<LeaveTypeResponseDto>>>("api/leaves/types");
            ViewBag.LeaveTypes = typesResponse?.Data ?? new List<LeaveTypeResponseDto>();

            var balanceResponse = await client.GetFromJsonAsync<ApiResponse<List<LeaveBalanceResponseDto>>>($"api/leaves/balances/employee/{employeeId}?year={year}");
            var existing = balanceResponse?.Data?.FirstOrDefault(b => b.LeaveTypeId == leaveTypeId);

            return View(new SetLeaveBalanceRequestDto
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                Year = year,
                TotalDays = existing?.TotalDays ?? 0,
                UsedDays = existing?.UsedDays ?? 0
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetBalance(SetLeaveBalanceRequestDto model)
        {
            if (ModelState.IsValid)
            {
                var client = _httpClientFactory.CreateClient("HRM_API");
                var response = await client.PostAsJsonAsync("api/leaves/balances", model);
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeaveBalanceResponseDto>>();

                if (result?.Success == true)
                    return RedirectToAction(nameof(Balances), new { employeeId = model.EmployeeId, year = model.Year });

                ModelState.AddModelError("", result?.Message ?? "Failed to set balance.");
            }

            var client2 = _httpClientFactory.CreateClient("HRM_API");
            var typesResponse = await client2.GetFromJsonAsync<ApiResponse<List<LeaveTypeResponseDto>>>("api/leaves/types");
            ViewBag.LeaveTypes = typesResponse?.Data ?? new List<LeaveTypeResponseDto>();
            return View(model);
        }
    }
}
