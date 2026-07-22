using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRM.Razor.Pages.Departments
{
    [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
    public class CreateModel : PageModel
    {
        private readonly IDepartmentApiClient _departmentApiClient;
        private readonly IEmployeeApiClient _employeeApiClient;

        public CreateModel(
            IDepartmentApiClient departmentApiClient,
            IEmployeeApiClient employeeApiClient)
        {
            _departmentApiClient = departmentApiClient;
            _employeeApiClient = employeeApiClient;
        }

        [BindProperty]
        public CreateDepartmentViewModel Input { get; set; } = new();

        public List<SelectListItem> ManagerOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "Departments";
            await LoadActiveManagersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActivePage"] = "Departments";

            if (!ModelState.IsValid)
            {
                await LoadActiveManagersAsync();
                return Page();
            }

            Input.DepartmentName = Input.DepartmentName?.Trim() ?? string.Empty;

            var response = await _departmentApiClient.CreateDepartmentAsync(Input);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (response.StatusCode == 403)
            {
                return RedirectToPage("/Account/AccessDenied");
            }

            if (!response.Success)
            {
                ModelState.AddModelError(string.Empty, response.Message ?? "Tạo phòng ban thất bại.");
                await LoadActiveManagersAsync();
                return Page();
            }

            TempData["SuccessMessage"] = $"Đã tạo thành công phòng ban {Input.DepartmentName}!";
            return RedirectToPage("/Departments/Index");
        }

        private async Task LoadActiveManagersAsync()
        {
            var empResponse = await _employeeApiClient.GetEmployeesAsync(pageSize: 100, status: "ACTIVE");
            if (empResponse.Success && empResponse.Data?.Items != null)
            {
                ManagerOptions = empResponse.Data.Items
                    .Select(e => new SelectListItem
                    {
                        Value = e.EmployeeId.ToString(),
                        Text = $"{e.FullName} ({e.EmployeeCode})"
                    })
                    .ToList();
            }

            ManagerOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Chưa bổ nhiệm Trưởng phòng --" });
        }
    }
}
