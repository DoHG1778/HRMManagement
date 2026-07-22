using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRM.Razor.Pages.Employees
{
    [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
    public class EditModel : PageModel
    {
        private readonly IEmployeeApiClient _employeeApiClient;

        public EditModel(IEmployeeApiClient employeeApiClient)
        {
            _employeeApiClient = employeeApiClient;
        }

        [BindProperty]
        public EditEmployeeViewModel Input { get; set; } = new();

        public EmployeeDetailModel EmployeeDetail { get; set; } = new();

        public List<SelectListItem> ManagerOptions { get; set; } = new();

        public bool IsAdmin { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            ViewData["ActivePage"] = "Employees";
            IsAdmin = CheckIsAdmin();

            if (id <= 0)
            {
                return RedirectToPage("/Employees/Index");
            }

            var response = await _employeeApiClient.GetEmployeeDetailAsync(id);
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
                TempData["ErrorMessage"] = "Không tìm thấy hồ sơ nhân viên yêu cầu.";
                return RedirectToPage("/Employees/Index");
            }

            EmployeeDetail = response.Data;
            MapDetailToViewModel(EmployeeDetail);

            await LoadDropdownsAsync(id);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            ViewData["ActivePage"] = "Employees";
            IsAdmin = CheckIsAdmin();

            if (!IsAdmin)
            {
                Input.SelectedRole = "Employee";
            }

            if (!ModelState.IsValid)
            {
                await ReloadPageDataAsync(id);
                return Page();
            }

            // 1. Trim dữ liệu và gọi API Cập nhật nhân viên
            Input.FullName = Input.FullName?.Trim() ?? string.Empty;
            Input.Email = Input.Email?.Trim() ?? string.Empty;

            var response = await _employeeApiClient.UpdateEmployeeAsync(id, Input);

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!response.Success)
            {
                var errorMessage = string.IsNullOrWhiteSpace(response.Message)
                    ? "Cập nhật hồ sơ nhân viên thất bại."
                    : response.Message;

                ModelState.AddModelError(string.Empty, errorMessage);
                await ReloadPageDataAsync(id);
                return Page();
            }

            TempData["SuccessMessage"] = $"Đã cập nhật thành công hồ sơ nhân viên {Input.FullName}!";
            return RedirectToPage("/Employees/Edit", new { id });
        }

        public async Task<IActionResult> OnPostChangeStatusAsync(int id, string targetStatus)
        {
            if (id <= 0 || string.IsNullOrWhiteSpace(targetStatus))
            {
                TempData["ErrorMessage"] = "Trạng thái thay đổi không hợp lệ.";
                return RedirectToPage("/Employees/Edit", new { id });
            }

            var response = await _employeeApiClient.ChangeEmployeeStatusAsync(id, targetStatus.Trim());

            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!response.Success)
            {
                TempData["ErrorMessage"] = response.Message ?? "Thay đổi trạng thái thất bại.";
                return RedirectToPage("/Employees/Edit", new { id });
            }

            var statusText = targetStatus switch
            {
                "ACTIVE" => "Đang làm việc",
                "ON_LEAVE" => "Đang nghỉ",
                "RESIGNED" => "Đã nghỉ việc",
                "TERMINATED" => "Đã chấm dứt",
                _ => targetStatus
            };

            TempData["SuccessMessage"] = $"Đã thay đổi trạng thái nhân viên sang: {statusText}.";
            return RedirectToPage("/Employees/Edit", new { id });
        }

        private bool CheckIsAdmin()
        {
            return User.IsInRole("Admin") || User.IsInRole("System Administrator");
        }

        private async Task ReloadPageDataAsync(int id)
        {
            var detailResponse = await _employeeApiClient.GetEmployeeDetailAsync(id);
            if (detailResponse.Success && detailResponse.Data != null)
            {
                EmployeeDetail = detailResponse.Data;
            }
            await LoadDropdownsAsync(id);
        }

        private void MapDetailToViewModel(EmployeeDetailModel detail)
        {
            Input = new EditEmployeeViewModel
            {
                EmployeeId = detail.EmployeeId,
                EmployeeCode = detail.EmployeeCode,
                FullName = detail.FullName,
                Gender = detail.Gender,
                DateOfBirth = detail.DateOfBirth,
                Phone = detail.Phone,
                Email = detail.Email,
                Address = detail.Address,
                Cccd = detail.Cccd,
                HireDate = detail.HireDate,
                EmploymentStatus = detail.EmploymentStatus,
                UserId = detail.UserId,
                ManagerId = detail.ManagerId,
                AvatarUrl = detail.AvatarUrl,
                SelectedRole = "Employee"
            };
        }

        private async Task LoadDropdownsAsync(int currentEmployeeId)
        {
            var empResponse = await _employeeApiClient.GetEmployeesAsync(pageSize: 100, status: "ACTIVE");
            if (empResponse.Success && empResponse.Data?.Items != null)
            {
                ManagerOptions = empResponse.Data.Items
                    .Where(e => e.EmployeeId != currentEmployeeId)
                    .Select(e => new SelectListItem
                    {
                        Value = e.EmployeeId.ToString(),
                        Text = $"{e.FullName} ({e.EmployeeCode})"
                    })
                    .ToList();
            }

            ManagerOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn người quản lý trực tiếp --" });
        }
    }
}
