using HRM.Razor.Models.ViewModels.Payroll;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRM.Razor.Pages.Payroll
{
    [Authorize(Roles = "Admin,Payroll,Payroll Officer")]
    public class GenerateModel : PageModel
    {
        private readonly IPayrollApiClient _payrollApiClient;
        private readonly IDepartmentApiClient _departmentApiClient;

        public GenerateModel(
            IPayrollApiClient payrollApiClient,
            IDepartmentApiClient departmentApiClient)
        {
            _payrollApiClient = payrollApiClient;
            _departmentApiClient = departmentApiClient;
        }

        [BindProperty]
        public GeneratePayrollViewModel Input { get; set; } = new();

        public List<SelectListItem> DepartmentOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "Payroll";
            await LoadDepartmentsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActivePage"] = "Payroll";

            if (!ModelState.IsValid)
            {
                await LoadDepartmentsAsync();
                return Page();
            }

            var response = await _payrollApiClient.GenerateMonthlyPayrollAsync(Input);

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
                ModelState.AddModelError(string.Empty, response.Message ?? "Tạo bảng lương tháng thất bại.");
                await LoadDepartmentsAsync();
                return Page();
            }

            TempData["SuccessMessage"] = response.Message ?? $"Đã khởi tạo bảng lương nháp tháng {Input.PayrollMonth}/{Input.PayrollYear} thành công!";
            return RedirectToPage("/Payroll/Calculate", new { month = Input.PayrollMonth, year = Input.PayrollYear });
        }

        private async Task LoadDepartmentsAsync()
        {
            var deptRes = await _departmentApiClient.GetDepartmentsAsync(isActive: true);
            if (deptRes.Success && deptRes.Data != null)
            {
                DepartmentOptions = deptRes.Data
                    .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName })
                    .ToList();
            }
            DepartmentOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Tất cả phòng ban --" });
        }
    }
}
