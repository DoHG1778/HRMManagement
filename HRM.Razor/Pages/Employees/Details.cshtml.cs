using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Employees
{
    [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator")]
    public class DetailsModel : PageModel
    {
        private readonly IEmployeeApiClient _employeeApiClient;

        public DetailsModel(IEmployeeApiClient employeeApiClient)
        {
            _employeeApiClient = employeeApiClient;
        }

        public EmployeeDetailModel Employee { get; set; } = new();

        public bool CanEdit { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ViewData["ActivePage"] = "Employees";

            if (id <= 0)
            {
                return RedirectToPage("/Employees/Index");
            }

            CanEdit = User.IsInRole("Admin") || User.IsInRole("HR") || User.IsInRole("HR Staff") || User.IsInRole("System Administrator");

            var response = await _employeeApiClient.GetEmployeeDetailAsync(id);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (!response.Success || response.Data == null)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không tìm thấy hồ sơ nhân viên yêu cầu.";
                return RedirectToPage("/Employees/Index");
            }

            Employee = response.Data;
            return Page();
        }
    }
}
