using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels;
using HRM.Razor.Models.ViewModels.Kpis;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.KPI
{
    [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator")]
    public class AssignModel : PageModel
    {
        private readonly IKpiApiClient _kpiApiClient;
        private readonly IEmployeeApiClient _employeeApiClient;

        public AssignModel(
            IKpiApiClient kpiApiClient,
            IEmployeeApiClient employeeApiClient)
        {
            _kpiApiClient = kpiApiClient;
            _employeeApiClient = employeeApiClient;
        }

        public List<KpiViewModel> Kpis { get; set; } = new();

        public List<EmployeeItemModel> Employees { get; set; } = new();

        [BindProperty]
        public AssignKpiViewModel Input { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "KPI";

            var kpiResponse =
                await _kpiApiClient.GetKpisAsync(true);

            var employeeResponse =
                await _employeeApiClient.GetEmployeesAsync(
                    pageNumber: 1,
                    pageSize: 100
                );

            if (kpiResponse.StatusCode == 401 ||
                employeeResponse.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");

                return RedirectToPage("/Account/Login");
            }

            if (kpiResponse.StatusCode == 403 ||
                employeeResponse.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (kpiResponse.Success &&
                kpiResponse.Data != null)
            {
                Kpis = kpiResponse.Data;
            }

            if (employeeResponse.Success &&
                employeeResponse.Data != null)
            {
                Employees = employeeResponse.Data.Items;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            var response =
                await _kpiApiClient.AssignKpiAsync(Input);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");

                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (response.Success)
            {
                TempData["SuccessMessage"] =
                    "Giao KPI cho nhân viên thành công.";

                return RedirectToPage();
            }

            TempData["ErrorMessage"] =
                response.Message ??
                "Giao KPI thất bại.";

            await LoadDataAsync();

            return Page();
        }

        private async Task LoadDataAsync()
        {
            var kpiResponse =
                await _kpiApiClient.GetKpisAsync(true);

            var employeeResponse =
                await _employeeApiClient.GetEmployeesAsync(
                    pageNumber: 1,
                    pageSize: 100
                );

            if (kpiResponse.Success &&
                kpiResponse.Data != null)
            {
                Kpis = kpiResponse.Data;
            }

            if (employeeResponse.Success &&
                employeeResponse.Data != null)
            {
                Employees = employeeResponse.Data.Items;
            }
        }
    }
}