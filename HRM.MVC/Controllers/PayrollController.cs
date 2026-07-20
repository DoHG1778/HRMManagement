using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IO;
using ClosedXML.Excel;
using HRM.Business.Common;
using HRM.Business.DTOs.Payrolls;
using HRM.MVC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HRM.MVC.Controllers
{
    public class PayrollController : Controller
    {
        private const string TokenSessionKey = "AccessToken";
        private readonly IHttpClientFactory _httpClientFactory;

        public PayrollController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        private bool HasToken()
        {
            return !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(TokenSessionKey));
        }

        private HttpClient? CreateAuthorizedClient()
        {
            var token = HttpContext.Session.GetString(TokenSessionKey);
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var client = _httpClientFactory.CreateClient("HRM_API");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        private IActionResult? RedirectOnAuthFailure<T>(ApiResponse<T>? response)
        {
            if (response?.StatusCode == 401)
            {
                HttpContext.Session.Clear();
                TempData["Error"] = "Your login session has expired. Please login again.";
                return RedirectToAction("Login", "Auth");
            }

            if (response?.StatusCode == 403)
            {
                TempData["Error"] = "You do not have permission to use this function.";
                return RedirectToAction("Index", "Home");
            }

            return null;
        }

        // GET: /Payroll
        public async Task<IActionResult> Index(PayrollFilterDto filter)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            filter.PageNumber = filter.PageNumber <= 0 ? 1 : filter.PageNumber;
            filter.PageSize = filter.PageSize <= 0 ? 10 : filter.PageSize;

            if (filter.PayrollMonth <= 0) filter.PayrollMonth = DateTime.Today.Month;
            if (filter.PayrollYear <= 0) filter.PayrollYear = DateTime.Today.Year;

            ViewBag.Filter = filter;

            var query = $"api/payrolls?payrollMonth={filter.PayrollMonth}&payrollYear={filter.PayrollYear}&pageNumber={filter.PageNumber}&pageSize={filter.PageSize}";
            if (!string.IsNullOrWhiteSpace(filter.Status)) query += $"&status={filter.Status}";
            if (filter.EmployeeId.HasValue) query += $"&employeeId={filter.EmployeeId}";
            if (filter.DepartmentId.HasValue) query += $"&departmentId={filter.DepartmentId}";

            var httpResponse = await client.GetAsync(query);
            var response = await ApiResponseReader.ReadAsync<PagedResult<PayrollResponseDto>>(httpResponse);
            
            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true)
            {
                TempData["Error"] = response?.Message ?? "Failed to load payroll list.";
            }

            return View(response?.Data ?? PagedResult<PayrollResponseDto>.Create(new List<PayrollResponseDto>(), filter.PageNumber, filter.PageSize, 0));
        }

        // GET: /Payroll/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.GetAsync($"api/payrolls/{id}");
            var response = await ApiResponseReader.ReadAsync<PayrollResponseDto>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true || response.Data == null)
            {
                TempData["Error"] = response?.Message ?? "Payroll detail not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(response.Data);
        }

        // POST: /Payroll/Generate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Generate(GeneratePayrollRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid month or year values.";
                return RedirectToAction(nameof(Index));
            }

            var httpResponse = await client.PostAsJsonAsync("api/payrolls/generate", model);
            var response = await ApiResponseReader.ReadAsync<List<PayrollResponseDto>>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success == true)
            {
                TempData["Success"] = response.Message ?? "Payrolls generated successfully.";
            }
            else
            {
                TempData["Error"] = response?.Message ?? "Failed to generate payrolls.";
            }

            return RedirectToAction(nameof(Index), new { payrollMonth = model.PayrollMonth, payrollYear = model.PayrollYear, employeeId = model.EmployeeId, departmentId = model.DepartmentId });
        }

        // POST: /Payroll/Calculate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Calculate(CalculatePayrollDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please fill in all calculation fields correctly.";
                return RedirectToAction(nameof(Index));
            }

            var httpResponse = await client.PostAsJsonAsync("api/payrolls/calculate", model);
            var response = await ApiResponseReader.ReadAsync<List<PayrollResponseDto>>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success == true)
            {
                TempData["Success"] = response.Message ?? "Payrolls calculated successfully.";
            }
            else
            {
                TempData["Error"] = response?.Message ?? "Failed to calculate payrolls.";
            }

            return RedirectToAction(nameof(Index), new { payrollMonth = model.PayrollMonth, payrollYear = model.PayrollYear, employeeId = model.EmployeeId, departmentId = model.DepartmentId });
        }

        // POST: /Payroll/Confirm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(ConfirmPayrollRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid month or year values for confirmation.";
                return RedirectToAction(nameof(Index));
            }

            var httpResponse = await client.PutAsJsonAsync("api/payrolls/confirm", model);
            var response = await ApiResponseReader.ReadAsync<bool>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success == true)
            {
                TempData["Success"] = response.Message ?? "Payrolls confirmed successfully.";
            }
            else
            {
                TempData["Error"] = response?.Message ?? "Failed to confirm payrolls.";
            }

            return RedirectToAction(nameof(Index), new { payrollMonth = model.PayrollMonth, payrollYear = model.PayrollYear, employeeId = model.EmployeeId, departmentId = model.DepartmentId });
        }

        // GET: /Payroll/MyPayslip
        public async Task<IActionResult> MyPayslip(int? month, int? year)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            int queryMonth = month ?? DateTime.Today.Month;
            int queryYear = year ?? DateTime.Today.Year;

            ViewBag.SelectedMonth = queryMonth;
            ViewBag.SelectedYear = queryYear;

            var httpResponse = await client.GetAsync($"api/payrolls/my-payslip?month={queryMonth}&year={queryYear}");
            var response = await ApiResponseReader.ReadAsync<PayrollResponseDto>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true)
            {
                ViewBag.ErrorMessage = response?.Message ?? $"No payslip found for {queryMonth}/{queryYear}.";
                return View(null);
            }

            return View(response.Data);
        }

        // POST: /Payroll/UpdateDetails/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateDetails(int id, UpdatePayrollDetailRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var httpResponse = await client.PutAsJsonAsync($"api/payrolls/{id}/details", model);
            var response = await ApiResponseReader.ReadAsync<PayrollResponseDto>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success == true)
            {
                TempData["Success"] = "Payroll details updated successfully.";
            }
            else
            {
                TempData["Error"] = response?.Message ?? "Failed to update payroll details.";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: /Payroll/Export
        public async Task<IActionResult> Export(int month, int year, int? departmentId, int? employeeId)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            var query = $"api/payrolls/export?month={month}&year={year}";
            if (departmentId.HasValue) query += $"&departmentId={departmentId}";
            if (employeeId.HasValue) query += $"&employeeId={employeeId}";

            var httpResponse = await client.GetAsync(query);
            var response = await ApiResponseReader.ReadAsync<PayrollReportDto>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success != true || response.Data == null)
            {
                TempData["Error"] = response?.Message ?? "Failed to export payroll report.";
                return RedirectToAction(nameof(Index), new { payrollMonth = month, payrollYear = year, employeeId = employeeId, departmentId = departmentId });
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Payroll Report");

                var headers = new string[]
                {
                    "STT", "Employee Code", "Employee Name", "Department", "Position",
                    "Payroll Month", "Payroll Year", "Base Salary", "Allowance", "Bonus",
                    "Overtime", "Other", "Deduction", "Gross Salary", "Net Salary", "Status", "Confirmed At"
                };

                for (int i = 0; i < headers.Length; i++)
                {
                    var cell = worksheet.Cell(1, i + 1);
                    cell.Value = headers[i];
                    cell.Style.Font.Bold = true;
                    cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }

                int row = 2;
                var list = response.Data.Payrolls ?? new List<PayrollResponseDto>();
                for (int idx = 0; idx < list.Count; idx++)
                {
                    var p = list[idx];
                    decimal otherEarnings = p.PayrollDetails?.Where(d => d.ItemType == "OTHER").Sum(d => d.Amount) ?? 0;
                    decimal deduction = p.PayrollDetails?.Where(d => d.ItemType == "DEDUCTION").Sum(d => d.Amount) ?? 0;

                    worksheet.Cell(row, 1).Value = idx + 1;
                    worksheet.Cell(row, 2).Value = p.EmployeeCode ?? "";
                    worksheet.Cell(row, 3).Value = p.EmployeeName ?? "";
                    worksheet.Cell(row, 4).Value = p.DepartmentName ?? "";
                    worksheet.Cell(row, 5).Value = p.PositionName ?? "";
                    worksheet.Cell(row, 6).Value = p.PayrollMonth;
                    worksheet.Cell(row, 7).Value = p.PayrollYear;
                    worksheet.Cell(row, 8).Value = p.BaseSalary;
                    worksheet.Cell(row, 9).Value = p.TotalAllowance;
                    worksheet.Cell(row, 10).Value = p.TotalBonus;
                    worksheet.Cell(row, 11).Value = p.TotalOvertime;
                    worksheet.Cell(row, 12).Value = otherEarnings;
                    worksheet.Cell(row, 13).Value = deduction;
                    worksheet.Cell(row, 14).Value = p.GrossSalary;
                    worksheet.Cell(row, 15).Value = p.NetSalary;
                    worksheet.Cell(row, 16).Value = p.Status;
                    worksheet.Cell(row, 17).Value = p.ConfirmedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "";

                    for (int col = 8; col <= 15; col++)
                    {
                        worksheet.Cell(row, col).Style.NumberFormat.Format = "#,##0.00";
                    }
                    row++;
                }

                int totalRow = row;
                worksheet.Cell(totalRow, 3).Value = "Total";
                worksheet.Cell(totalRow, 3).Style.Font.Bold = true;

                worksheet.Cell(totalRow, 8).FormulaA1 = $"=SUM(H2:H{totalRow - 1})";
                worksheet.Cell(totalRow, 9).FormulaA1 = $"=SUM(I2:I{totalRow - 1})";
                worksheet.Cell(totalRow, 10).FormulaA1 = $"=SUM(J2:J{totalRow - 1})";
                worksheet.Cell(totalRow, 11).FormulaA1 = $"=SUM(K2:K{totalRow - 1})";
                worksheet.Cell(totalRow, 12).FormulaA1 = $"=SUM(L2:L{totalRow - 1})";
                worksheet.Cell(totalRow, 13).FormulaA1 = $"=SUM(M2:M{totalRow - 1})";
                worksheet.Cell(totalRow, 14).FormulaA1 = $"=SUM(N2:N{totalRow - 1})";
                worksheet.Cell(totalRow, 15).FormulaA1 = $"=SUM(O2:O{totalRow - 1})";

                for (int col = 8; col <= 15; col++)
                {
                    worksheet.Cell(totalRow, col).Style.Font.Bold = true;
                    worksheet.Cell(totalRow, col).Style.NumberFormat.Format = "#,##0.00";
                }

                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Payroll_Report_{month}_{year}.xlsx");
                }
            }
        }

        // POST: /Payroll/Pay
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Pay(ConfirmPayrollRequestDto model)
        {
            var client = CreateAuthorizedClient();
            if (client == null) return RedirectToAction("Login", "Auth");

            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid month or year values for payment confirmation.";
                return RedirectToAction(nameof(Index));
            }

            var httpResponse = await client.PutAsJsonAsync("api/payrolls/pay", model);
            var response = await ApiResponseReader.ReadAsync<bool>(httpResponse);

            var authFailure = RedirectOnAuthFailure(response);
            if (authFailure != null) return authFailure;

            if (response?.Success == true)
            {
                TempData["Success"] = response.Message ?? "Payrolls marked as paid successfully.";
            }
            else
            {
                TempData["Error"] = response?.Message ?? "Failed to mark payrolls as paid.";
            }

            return RedirectToAction(nameof(Index), new { payrollMonth = model.PayrollMonth, payrollYear = model.PayrollYear, employeeId = model.EmployeeId, departmentId = model.DepartmentId });
        }
    }
}
