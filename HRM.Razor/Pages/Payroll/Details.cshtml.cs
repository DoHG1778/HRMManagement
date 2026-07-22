using HRM.Razor.Models.ViewModels.Payroll;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;

namespace HRM.Razor.Pages.Payroll
{
    [Authorize(Roles = "Admin,Payroll,Payroll Officer")]
    public class DetailsModel : PageModel
    {
        private readonly IPayrollApiClient _payrollApiClient;

        public DetailsModel(IPayrollApiClient payrollApiClient)
        {
            _payrollApiClient = payrollApiClient;
        }

        public PayrollItemModel Payroll { get; set; } = new();

        public List<PayrollDetailItemModel> IncomeItems { get; set; } = new();
        public List<PayrollDetailItemModel> DeductionItems { get; set; } = new();

        public bool IsDraft => Payroll.Status == "DRAFT";

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ViewData["ActivePage"] = "Payroll";

            if (id <= 0)
            {
                return RedirectToPage("/Payroll/Index");
            }

            var response = await _payrollApiClient.GetPayrollByIdAsync(id);

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
                TempData["ErrorMessage"] = "Không tìm thấy thông tin bảng lương yêu cầu.";
                return RedirectToPage("/Payroll/Index");
            }

            Payroll = response.Data;
            CategorizePayrollDetails();

            return Page();
        }

        public async Task<IActionResult> OnPostSaveDetailsAsync(int id, string itemsJson)
        {
            ViewData["ActivePage"] = "Payroll";

            if (id <= 0)
            {
                return RedirectToPage("/Payroll/Index");
            }

            if (string.IsNullOrWhiteSpace(itemsJson))
            {
                TempData["ErrorMessage"] = "Dữ liệu chi tiết bảng lương không hợp lệ.";
                return RedirectToPage("/Payroll/Details", new { id });
            }

            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var items = JsonSerializer.Deserialize<List<PayrollDetailItemModel>>(itemsJson, options) ?? new List<PayrollDetailItemModel>();

                var response = await _payrollApiClient.UpdatePayrollDetailAsync(id, items);

                if (response.StatusCode == 401)
                {
                    HttpContext.Session.Remove("JWToken");
                    return RedirectToPage("/Account/Login");
                }

                if (!response.Success)
                {
                    TempData["ErrorMessage"] = response.Message ?? "Cập nhật chi tiết bảng lương thất bại.";
                }
                else
                {
                    TempData["SuccessMessage"] = "Đã cập nhật chi tiết bảng lương và tự động tính lại tổng lương thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi xử lý dữ liệu chi tiết: " + ex.Message;
            }

            return RedirectToPage("/Payroll/Details", new { id });
        }

        private void CategorizePayrollDetails()
        {
            if (Payroll.PayrollDetails != null && Payroll.PayrollDetails.Any())
            {
                IncomeItems = Payroll.PayrollDetails
                    .Where(d => d.ItemType == "ALLOWANCE" || d.ItemType == "BONUS" || d.ItemType == "OVERTIME" || d.ItemType == "OTHER")
                    .ToList();

                DeductionItems = Payroll.PayrollDetails
                    .Where(d => d.ItemType == "DEDUCTION")
                    .ToList();
            }
        }
    }
}
