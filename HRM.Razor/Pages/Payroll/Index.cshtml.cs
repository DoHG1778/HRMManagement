using HRM.Razor.Models.ViewModels.Payroll;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRM.Razor.Pages.Payroll
{
    [Authorize(Roles = "Admin,Payroll,Payroll Officer")]
    public class IndexModel : PageModel
    {
        private readonly IPayrollApiClient _payrollApiClient;
        private readonly IDepartmentApiClient _departmentApiClient;

        public IndexModel(
            IPayrollApiClient payrollApiClient,
            IDepartmentApiClient departmentApiClient)
        {
            _payrollApiClient = payrollApiClient;
            _departmentApiClient = departmentApiClient;
        }

        public List<PayrollItemModel> Payrolls { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? Month { get; set; } = DateTime.Today.Month;

        [BindProperty(SupportsGet = true)]
        public int? Year { get; set; } = DateTime.Today.Year;

        [BindProperty(SupportsGet = true)]
        public int? DepartmentId { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Status { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasPreviousPage { get; set; }
        public bool HasNextPage { get; set; }

        // Statistics
        public int DraftCount { get; set; }
        public int ConfirmedCount { get; set; }
        public int PaidCount { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public decimal TotalNetAmount { get; set; }

        public List<SelectListItem> DepartmentOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "Payroll";

            var deptRes = await _departmentApiClient.GetDepartmentsAsync(isActive: true);
            if (deptRes.Success && deptRes.Data != null)
            {
                DepartmentOptions = deptRes.Data
                    .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName })
                    .ToList();
            }
            DepartmentOptions.Insert(0, new SelectListItem { Value = "", Text = "Tất cả phòng ban" });

            var response = await _payrollApiClient.GetPayrollsAsync(
                month: Month,
                year: Year,
                departmentId: DepartmentId,
                status: Status,
                pageNumber: PageNumber,
                pageSize: 10);

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
                Payrolls = response.Data.Items;
                TotalItems = response.Data.TotalItems;
                TotalPages = response.Data.TotalPages;
                HasPreviousPage = response.Data.HasPreviousPage;
                HasNextPage = response.Data.HasNextPage;

                DraftCount = Payrolls.Count(p => p.Status == "DRAFT");
                ConfirmedCount = Payrolls.Count(p => p.Status == "CONFIRMED");
                PaidCount = Payrolls.Count(p => p.Status == "PAID");
                TotalGrossAmount = Payrolls.Sum(p => p.GrossSalary);
                TotalNetAmount = Payrolls.Sum(p => p.NetSalary);
            }

            return Page();
        }
    }
}
