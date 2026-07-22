using HRM.Razor.Models.ViewModels.Payroll;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRM.Razor.Pages.Payroll
{
    [Authorize(Roles = "Admin,Payroll,Payroll Officer")]
    public class ConfirmModel : PageModel
    {
        private readonly IPayrollApiClient _payrollApiClient;
        private readonly IDepartmentApiClient _departmentApiClient;

        public ConfirmModel(
            IPayrollApiClient payrollApiClient,
            IDepartmentApiClient departmentApiClient)
        {
            _payrollApiClient = payrollApiClient;
            _departmentApiClient = departmentApiClient;
        }

        [BindProperty(SupportsGet = true)]
        public int Month { get; set; } = DateTime.Today.Month;

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; } = DateTime.Today.Year;

        [BindProperty(SupportsGet = true)]
        public int? DepartmentId { get; set; }

        public List<PayrollItemModel> Payrolls { get; set; } = new();
        public List<SelectListItem> DepartmentOptions { get; set; } = new();

        // Statistics Summary
        public int TotalEmployeesCount { get; set; }
        public int DraftCount { get; set; }
        public int ConfirmedCount { get; set; }
        public decimal TotalGrossSalarySum { get; set; }
        public decimal TotalNetSalarySum { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "Payroll";
            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostConfirmPeriodAsync()
        {
            ViewData["ActivePage"] = "Payroll";

            var response = await _payrollApiClient.ConfirmPayrollAsync(Month, Year, DepartmentId);

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
                TempData["ErrorMessage"] = response.Message ?? "Chốt bảng lương thất bại. Vui lòng kiểm tra lại dữ liệu.";
                await LoadDataAsync();
                return Page();
            }

            TempData["SuccessMessage"] = response.Message ?? $"Đã chốt bảng lương thành công cho kỳ {Month}/{Year}!";
            return RedirectToPage("/Payroll/Reports", new { month = Month, year = Year });
        }

        private async Task LoadDataAsync()
        {
            var deptRes = await _departmentApiClient.GetDepartmentsAsync(isActive: true);
            if (deptRes.Success && deptRes.Data != null)
            {
                DepartmentOptions = deptRes.Data
                    .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName })
                    .ToList();
            }
            DepartmentOptions.Insert(0, new SelectListItem { Value = "", Text = "Tất cả phòng ban" });

            var payrollRes = await _payrollApiClient.GetPayrollsAsync(
                month: Month,
                year: Year,
                departmentId: DepartmentId,
                pageNumber: 1,
                pageSize: 200);

            if (payrollRes.Success && payrollRes.Data != null)
            {
                Payrolls = payrollRes.Data.Items;
                TotalEmployeesCount = Payrolls.Count;
                DraftCount = Payrolls.Count(p => p.Status == "DRAFT");
                ConfirmedCount = Payrolls.Count(p => p.Status == "CONFIRMED" || p.Status == "PAID");
                TotalGrossSalarySum = Payrolls.Sum(p => p.GrossSalary);
                TotalNetSalarySum = Payrolls.Sum(p => p.NetSalary);
            }
        }
    }
}
