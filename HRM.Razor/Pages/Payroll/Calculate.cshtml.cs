using HRM.Razor.Models.ViewModels.Payroll;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRM.Razor.Pages.Payroll
{
    [Authorize(Roles = "Admin,Payroll,Payroll Officer")]
    public class CalculateModel : PageModel
    {
        private readonly IPayrollApiClient _payrollApiClient;
        private readonly IDepartmentApiClient _departmentApiClient;

        public CalculateModel(
            IPayrollApiClient payrollApiClient,
            IDepartmentApiClient departmentApiClient)
        {
            _payrollApiClient = payrollApiClient;
            _departmentApiClient = departmentApiClient;
        }

        [BindProperty]
        public CalculatePayrollViewModel Input { get; set; } = new();

        public List<PayrollItemModel> DraftPayrolls { get; set; } = new();
        public List<SelectListItem> DepartmentOptions { get; set; } = new();

        // Statistics
        public int TotalDraftCount { get; set; }
        public decimal TotalBaseSalarySum { get; set; }
        public decimal TotalGrossSalarySum { get; set; }
        public decimal TotalNetSalarySum { get; set; }

        public async Task<IActionResult> OnGetAsync(int? month, int? year, int? departmentId)
        {
            ViewData["ActivePage"] = "PayrollCalculate";

            if (month.HasValue && month.Value >= 1 && month.Value <= 12)
            {
                Input.PayrollMonth = month.Value;
            }

            if (year.HasValue && year.Value >= 2000 && year.Value <= 2100)
            {
                Input.PayrollYear = year.Value;
            }

            if (departmentId.HasValue)
            {
                Input.DepartmentId = departmentId;
            }

            await LoadPageDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActivePage"] = "PayrollCalculate";

            if (!ModelState.IsValid)
            {
                await LoadPageDataAsync();
                return Page();
            }

            var response = await _payrollApiClient.CalculatePayrollAsync(Input);

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
                ModelState.AddModelError(string.Empty, response.Message ?? "Tính lương thất bại.");
                await LoadPageDataAsync();
                return Page();
            }

            TempData["SuccessMessage"] = response.Message ?? $"Đã tự động tính lương thành công cho kỳ {Input.PayrollMonth}/{Input.PayrollYear}!";
            return RedirectToPage("/Payroll/Calculate", new { month = Input.PayrollMonth, year = Input.PayrollYear, departmentId = Input.DepartmentId });
        }

        private async Task LoadPageDataAsync()
        {
            var deptRes = await _departmentApiClient.GetDepartmentsAsync(isActive: true);
            if (deptRes.Success && deptRes.Data != null)
            {
                DepartmentOptions = deptRes.Data
                    .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName })
                    .ToList();
            }
            DepartmentOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Tất cả phòng ban --" });

            var payrollRes = await _payrollApiClient.GetPayrollsAsync(
                month: Input.PayrollMonth,
                year: Input.PayrollYear,
                departmentId: Input.DepartmentId,
                status: "DRAFT",
                pageNumber: 1,
                pageSize: 100);

            if (payrollRes.Success && payrollRes.Data != null)
            {
                DraftPayrolls = payrollRes.Data.Items;
                TotalDraftCount = DraftPayrolls.Count;
                TotalBaseSalarySum = DraftPayrolls.Sum(p => p.BaseSalary);
                TotalGrossSalarySum = DraftPayrolls.Sum(p => p.GrossSalary);
                TotalNetSalarySum = DraftPayrolls.Sum(p => p.NetSalary);
            }
        }
    }
}
