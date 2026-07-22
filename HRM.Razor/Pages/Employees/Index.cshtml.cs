using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Employees
{
    [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator")]
    public class IndexModel : PageModel
    {
        private readonly IEmployeeApiClient _employeeApiClient;

        public IndexModel(IEmployeeApiClient employeeApiClient)
        {
            _employeeApiClient = employeeApiClient;
        }

        public PagedResultModel<EmployeeItemModel> Employees { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "Employees";

            var response = await _employeeApiClient.GetEmployeesAsync(
                keyword: Keyword,
                status: Status,
                pageNumber: PageNumber,
                pageSize: 10
            );

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (response.Success && response.Data != null)
            {
                Employees = response.Data;
            }

            return Page();
        }
    }
}
