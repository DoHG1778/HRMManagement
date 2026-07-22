using System.Text;
using HRM.Razor.Models.ViewModels.Payroll;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRM.Razor.Pages.Payroll
{
    [Authorize(Roles = "Admin,Payroll,Payroll Officer")]
    public class ReportsModel : PageModel
    {
        private readonly IPayrollApiClient _payrollApiClient;
        private readonly IDepartmentApiClient _departmentApiClient;

        public ReportsModel(
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

        public PayrollReportViewModel Report { get; set; } = new();
        public List<SelectListItem> DepartmentOptions { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "Payroll";
            await LoadReportDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnGetExportCsvAsync(int month, int year, int? departmentId)
        {
            Month = month;
            Year = year;
            DepartmentId = departmentId;

            var response = await _payrollApiClient.GetPayrollReportAsync(Month, Year, DepartmentId);
            if (!response.Success || response.Data == null || response.Data.Payrolls == null)
            {
                TempData["ErrorMessage"] = "Không có dữ liệu báo cáo để xuất file.";
                return RedirectToPage("/Payroll/Reports", new { month = Month, year = Year, departmentId = DepartmentId });
            }

            var reportData = response.Data;
            var sb = new StringBuilder();

            // UTF-8 BOM for Excel compatibility
            sb.Append('\uFEFF');

            // Header info
            sb.AppendLine($"BÁO CÁO BẢNG LƯƠNG - THÁNG {Month}/{Year}");
            sb.AppendLine($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Tổng số nhân viên: {reportData.TotalEmployees}");
            sb.AppendLine($"Tổng lương cơ bản: {reportData.TotalBaseSalary:N0}");
            sb.AppendLine($"Tổng thu nhập (Gross): {reportData.TotalGrossSalary:N0}");
            sb.AppendLine($"Tổng khấu trừ: {reportData.TotalDeduction:N0}");
            sb.AppendLine($"Tổng thực lĩnh (Net): {reportData.TotalNetSalary:N0}");
            sb.AppendLine();

            // CSV Columns
            sb.AppendLine("STT,Mã NV,Họ tên nhân viên,Phòng ban,Chức vụ,Lương cơ bản,Phụ cấp,Thưởng,Tăng ca (OT),Tổng thu nhập (Gross),Tổng khấu trừ,Thực lĩnh (Net),Trạng thái");

            int index = 1;
            foreach (var item in reportData.Payrolls)
            {
                var line = string.Join(",", new[]
                {
                    index.ToString(),
                    EscapeCsvField(item.EmployeeCode),
                    EscapeCsvField(item.EmployeeName),
                    EscapeCsvField(item.DepartmentName ?? "Chưa phân công"),
                    EscapeCsvField(item.PositionName ?? "Chưa phân công"),
                    item.BaseSalary.ToString("F0"),
                    item.TotalAllowance.ToString("F0"),
                    item.TotalBonus.ToString("F0"),
                    item.TotalOvertime.ToString("F0"),
                    item.GrossSalary.ToString("F0"),
                    item.TotalDeduction.ToString("F0"),
                    item.NetSalary.ToString("F0"),
                    EscapeCsvField(item.Status)
                });
                sb.AppendLine(line);
                index++;
            }

            var fileName = $"BaoCaoLuong_Thang{Month:D2}_{Year}.csv";
            var fileBytes = Encoding.UTF8.GetBytes(sb.ToString());
            return File(fileBytes, "text/csv; charset=utf-8", fileName);
        }

        private async Task LoadReportDataAsync()
        {
            var deptRes = await _departmentApiClient.GetDepartmentsAsync(isActive: true);
            if (deptRes.Success && deptRes.Data != null)
            {
                DepartmentOptions = deptRes.Data
                    .Select(d => new SelectListItem { Value = d.DepartmentId.ToString(), Text = d.DepartmentName })
                    .ToList();
            }
            DepartmentOptions.Insert(0, new SelectListItem { Value = "", Text = "Tất cả phòng ban" });

            var response = await _payrollApiClient.GetPayrollReportAsync(Month, Year, DepartmentId);
            if (response.Success && response.Data != null)
            {
                Report = response.Data;

                // Group by department for summary tab
                if (Report.Payrolls != null && Report.Payrolls.Any())
                {
                    Report.DepartmentSummaries = Report.Payrolls
                        .GroupBy(p => p.DepartmentName ?? "Chưa phân công")
                        .Select(g => new DepartmentPayrollSummaryModel
                        {
                            DepartmentName = g.Key,
                            EmployeeCount = g.Count(),
                            TotalBaseSalary = g.Sum(x => x.BaseSalary),
                            TotalGrossSalary = g.Sum(x => x.GrossSalary),
                            TotalDeduction = g.Sum(x => x.TotalDeduction),
                            TotalNetSalary = g.Sum(x => x.NetSalary)
                        })
                        .OrderBy(d => d.DepartmentName)
                        .ToList();
                }
            }
        }

        private static string EscapeCsvField(string? field)
        {
            if (string.IsNullOrEmpty(field)) return "\"\"";
            if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return $"\"{field}\"";
        }
    }
}
