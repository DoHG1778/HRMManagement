using System.Net.Http.Headers;
using System.Net.Http.Json;
using HRM.Business.Common;
using HRM.Business.DTOs.Contracts;
using HRM.Business.DTOs.Employees;
using HRM.MVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class ContractManagementController : Controller
    {
        private const string TokenSessionKey = "AccessToken";
        private readonly IHttpClientFactory _httpClientFactory;

        public ContractManagementController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> Index(ContractFilterDto filter)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            filter.PageNumber = filter.PageNumber == 0 ? 1 : filter.PageNumber;
            filter.PageSize = filter.PageSize == 0 ? 10 : filter.PageSize;

            var queryString = $"?pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
            if (filter.EmployeeId.HasValue) queryString += $"&employeeId={filter.EmployeeId}";
            if (!string.IsNullOrEmpty(filter.Status)) queryString += $"&status={filter.Status}";
            if (!string.IsNullOrEmpty(filter.Keyword)) queryString += $"&keyword={Uri.EscapeDataString(filter.Keyword)}";

            var httpResponse = await client.GetAsync($"api/contracts{queryString}");
            var response = await ApiResponseReader.ReadAsync<PagedResult<ContractResponseDto>>(httpResponse);

            if (response?.StatusCode == 401) return RedirectToAction("Login", "Auth");

            ViewBag.Filter = filter;
            return View(response?.Data ?? PagedResult<ContractResponseDto>.Create(new List<ContractResponseDto>(), filter.PageNumber, filter.PageSize, 0));
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync($"api/contracts/{id}");
            var response = await ApiResponseReader.ReadAsync<ContractResponseDto>(httpResponse);

            if (response?.Success != true)
            {
                TempData["Error"] = response?.Message ?? "Contract not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        public async Task<IActionResult> Create()
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            await LoadEmployees(client);
            return View(new CreateContractRequestDto { StartDate = DateOnly.FromDateTime(DateTime.Now) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateContractRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                await LoadEmployees(client);
                return View(model);
            }

            var response = await client.PostAsJsonAsync("api/contracts", model);
            var result = await ApiResponseReader.ReadAsync<ContractResponseDto>(response);

            if (result?.Success == true)
            {
                TempData["Success"] = "Contract created successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to create contract.");
            await LoadEmployees(client);
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync($"api/contracts/{id}");
            var response = await ApiResponseReader.ReadAsync<ContractResponseDto>(httpResponse);

            if (response?.Success != true)
            {
                TempData["Error"] = response?.Message ?? "Contract not found.";
                return RedirectToAction(nameof(Index));
            }

            var contract = response.Data;
            var model = new UpdateContractRequestDto
            {
                ContractType = contract.ContractType,
                StartDate = contract.StartDate,
                EndDate = contract.EndDate,
                Salary = contract.Salary,
                ContractFileUrl = contract.ContractFileUrl,
                Note = "" 
            };

            ViewBag.EmployeeName = contract.EmployeeName;
            ViewBag.ContractId = id;
            ViewBag.CreatedAt = contract.CreatedAt;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateContractRequestDto model)
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                var contractResponse = await client.GetAsync($"api/contracts/{id}");
                var contractData = await ApiResponseReader.ReadAsync<ContractResponseDto>(contractResponse);
                if (contractData?.Success == true)
                {
                    ViewBag.EmployeeName = contractData.Data.EmployeeName;
                    ViewBag.ContractId = id;
                    ViewBag.CreatedAt = contractData.Data.CreatedAt;
                }
                return View(model);
            }

            var response = await client.PutAsJsonAsync($"api/contracts/{id}", model);
            var result = await ApiResponseReader.ReadAsync<ContractResponseDto>(response);

            if (result?.Success == true)
            {
                TempData["Success"] = "Contract updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.AddModelError("", result?.Message ?? "Failed to update contract.");
            
            var cResponse = await client.GetAsync($"api/contracts/{id}");
            var cData = await ApiResponseReader.ReadAsync<ContractResponseDto>(cResponse);
            if (cData?.Success == true)
            {
                ViewBag.EmployeeName = cData.Data.EmployeeName;
                ViewBag.ContractId = id;
                ViewBag.CreatedAt = cData.Data.CreatedAt;
            }
            
            return View(model);
        }

        // UC15: Extend (Gia hạn) - GET
        public async Task<IActionResult> Extend(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync($"api/contracts/{id}");
            var response = await ApiResponseReader.ReadAsync<ContractResponseDto>(httpResponse);

            if (response?.Success != true) return RedirectToAction(nameof(Index));

            ViewBag.EmployeeName = response.Data.EmployeeName;
            ViewBag.CurrentEndDate = response.Data.EndDate;

            return View(new ExtendContractRequestDto { NewEndDate = response.Data.EndDate ?? DateOnly.FromDateTime(DateTime.Now.AddYears(1)) });
        }

        // UC15: Extend (Gia hạn) - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Extend(int id, ExtendContractRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var response = await client.PutAsJsonAsync($"api/contracts/{id}/extend", model);
            var result = await ApiResponseReader.ReadAsync<ContractResponseDto>(response);

            if (result?.Success == true)
            {
                TempData["Success"] = "Contract extended successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Error"] = result?.Message ?? "Failed to extend contract.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // UC15: Terminate (Chấm dứt) - GET
        public async Task<IActionResult> Terminate(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync($"api/contracts/{id}");
            var response = await ApiResponseReader.ReadAsync<ContractResponseDto>(httpResponse);

            if (response?.Success != true) return RedirectToAction(nameof(Index));

            ViewBag.EmployeeName = response.Data.EmployeeName;

            return View(new TerminateContractRequestDto { TerminationDate = DateOnly.FromDateTime(DateTime.Now) });
        }

        // UC15: Terminate (Chấm dứt) - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Terminate(int id, TerminateContractRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var response = await client.PutAsJsonAsync($"api/contracts/{id}/terminate", model);
            var result = await ApiResponseReader.ReadAsync<ContractResponseDto>(response);

            if (result?.Success == true)
            {
                TempData["Success"] = "Contract terminated successfully.";
                return RedirectToAction(nameof(Details), new { id });
            }

            TempData["Error"] = result?.Message ?? "Failed to terminate contract.";
            return RedirectToAction(nameof(Details), new { id });
        }

        private async Task LoadEmployees(HttpClient client)
        {
            var empResponse = await client.GetAsync("api/employees?pageSize=1000");
            var employees = await ApiResponseReader.ReadAsync<PagedResult<EmployeeResponseDto>>(empResponse);
            ViewBag.Employees = employees?.Data?.Items ?? new List<EmployeeResponseDto>();
        }

        private bool IsAdmin()
        {
            var roles = HttpContext.Session.GetString("Roles") ?? "";
            return roles.Split(',').Any(r => r.Trim().Equals("Admin", StringComparison.OrdinalIgnoreCase));
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