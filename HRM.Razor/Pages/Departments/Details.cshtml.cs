using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Departments
{
    [Authorize(Roles = "Admin,HR,HR Staff,Manager,Department Manager,System Administrator")]
    public class DetailsModel : PageModel
    {
        private readonly IDepartmentApiClient _departmentApiClient;

        public DetailsModel(IDepartmentApiClient departmentApiClient)
        {
            _departmentApiClient = departmentApiClient;
        }

        public DepartmentModel Department { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ViewData["ActivePage"] = "Departments";

            if (id <= 0)
            {
                return RedirectToPage("/Departments/Index");
            }

            var response = await _departmentApiClient.GetDepartmentByIdAsync(id);

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
                TempData["ErrorMessage"] = "Không tìm thấy thông tin phòng ban yêu cầu.";
                return RedirectToPage("/Departments/Index");
            }

            Department = response.Data;
            return Page();
        }
    }
}
