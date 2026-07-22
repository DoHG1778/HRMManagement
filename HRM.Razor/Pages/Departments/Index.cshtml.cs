using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Departments
{
    [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
    public class IndexModel : PageModel
    {
        private readonly IDepartmentApiClient _departmentApiClient;

        public IndexModel(IDepartmentApiClient departmentApiClient)
        {
            _departmentApiClient = departmentApiClient;
        }

        public List<DepartmentModel> Departments { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? Keyword { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? IsActive { get; set; }

        // Statistics
        public int TotalDepartments { get; set; }
        public int ActiveDepartments { get; set; }
        public int InactiveDepartments { get; set; }
        public int NoManagerDepartments { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "Departments";

            var response = await _departmentApiClient.GetDepartmentsAsync(IsActive);

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
                var allList = response.Data;

                // Thống kê
                TotalDepartments = allList.Count;
                ActiveDepartments = allList.Count(d => d.IsActive);
                InactiveDepartments = allList.Count(d => !d.IsActive);
                NoManagerDepartments = allList.Count(d => !d.ManagerEmployeeId.HasValue);

                // Lọc theo keyword nếu có
                if (!string.IsNullOrWhiteSpace(Keyword))
                {
                    var kw = Keyword.Trim().ToLower();
                    Departments = allList
                        .Where(d => d.DepartmentName.ToLower().Contains(kw) || (d.Description != null && d.Description.ToLower().Contains(kw)))
                        .ToList();
                }
                else
                {
                    Departments = allList;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostToggleStatusAsync(int id, bool targetIsActive)
        {
            if (id <= 0)
            {
                return RedirectToPage("/Departments/Index");
            }

            var response = targetIsActive
                ? await _departmentApiClient.ActivateDepartmentAsync(id)
                : await _departmentApiClient.DeactivateDepartmentAsync(id);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message ?? "Không thể thay đổi trạng thái phòng ban.";
            }
            else
            {
                TempData["SuccessMessage"] = targetIsActive
                    ? "Đã kích hoạt lại phòng ban thành công!"
                    : "Đã chuyển phòng ban sang trạng thái ngừng sử dụng.";
            }

            return RedirectToPage("/Departments/Index");
        }
    }
}
