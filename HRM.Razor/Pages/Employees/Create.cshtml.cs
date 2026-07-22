using HRM.Razor.Models.ViewModels;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRM.Razor.Pages.Employees
{
    [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
    public class CreateModel : PageModel
    {
        private readonly IEmployeeApiClient _employeeApiClient;

        public CreateModel(IEmployeeApiClient employeeApiClient)
        {
            _employeeApiClient = employeeApiClient;
        }

        [BindProperty]
        public CreateEmployeeViewModel Input { get; set; } = new();

        public List<SelectListItem> ManagerOptions { get; set; } = new();

        public bool IsAdmin { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            ViewData["ActivePage"] = "Employees";
            IsAdmin = CheckIsAdmin();

            if (!IsAdmin)
            {
                Input.SelectedRole = "Employee";
            }

            await LoadDropdownsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActivePage"] = "Employees";
            IsAdmin = CheckIsAdmin();

            // Nếu không phải Admin, khóa vai trò chọn về Employee
            if (!IsAdmin)
            {
                Input.SelectedRole = "Employee";
            }

            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return Page();
            }

            // 1. Trim dữ liệu và gọi API tạo hồ sơ nhân viên
            Input.EmployeeCode = Input.EmployeeCode?.Trim() ?? string.Empty;
            Input.FullName = Input.FullName?.Trim() ?? string.Empty;
            Input.Email = Input.Email?.Trim() ?? string.Empty;

            var response = await _employeeApiClient.CreateEmployeeAsync(Input);

            // 2. Xử lý Token hết hạn hoặc 401
            if (response.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            // 3. Xử lý thất bại
            if (!response.Success)
            {
                var errorMessage = string.IsNullOrWhiteSpace(response.Message)
                    ? "Tạo hồ sơ nhân viên thất bại. Vui lòng kiểm tra lại thông tin."
                    : response.Message;

                ModelState.AddModelError(string.Empty, errorMessage);
                await LoadDropdownsAsync();
                return Page();
            }

            // 4. Thành công -> Chuyển về danh sách nhân viên kèm thông báo
            TempData["SuccessMessage"] = $"Đã tạo thành công hồ sơ nhân viên {Input.FullName} ({Input.EmployeeCode}) với vai trò {(Input.SelectedRole == "Employee" ? "Nhân viên" : Input.SelectedRole)}!";
            return RedirectToPage("/Employees/Index");
        }

        private bool CheckIsAdmin()
        {
            return User.IsInRole("Admin") || User.IsInRole("System Administrator");
        }

        private async Task LoadDropdownsAsync()
        {
            var empResponse = await _employeeApiClient.GetEmployeesAsync(pageSize: 100, status: "ACTIVE");
            if (empResponse.Success && empResponse.Data?.Items != null)
            {
                ManagerOptions = empResponse.Data.Items
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
