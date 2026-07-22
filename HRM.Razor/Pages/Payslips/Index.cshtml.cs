using HRM.Razor.Models.ViewModels.Payroll;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HRM.Razor.Pages.Payslips
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IPayrollApiClient _payrollApiClient;

        public IndexModel(IPayrollApiClient payrollApiClient)
        {
            _payrollApiClient = payrollApiClient;
        }

        [BindProperty(SupportsGet = true)]
        public int Month { get; set; } = DateTime.Today.Month;

        [BindProperty(SupportsGet = true)]
        public int Year { get; set; } = DateTime.Today.Year;

        public PayrollItemModel? Payslip { get; set; }
        public string? ErrorText { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "MyPayslip";

            var response = await _payrollApiClient.GetMyPayslipAsync(Month, Year);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!response.Success || response.Data == null)
            {
                ErrorText = response.Message ?? $"Chưa có phiếu lương được xác nhận cho kỳ Tháng {Month}/{Year}.";
            }
            else
            {
                Payslip = response.Data;
            }

            return Page();
        }
    }
}
