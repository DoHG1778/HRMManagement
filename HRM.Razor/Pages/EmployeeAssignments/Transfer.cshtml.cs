using HRM.Razor.Models.ViewModels;
using HRM.Razor.Models.ViewModels.EmployeeAssignments;
using HRM.Razor.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HRM.Razor.Pages.EmployeeAssignments
{
    [Authorize(Roles = "Admin,HR,HR Staff,System Administrator")]
    public class TransferModel : PageModel
    {
        private readonly IEmployeeApiClient _employeeApiClient;
        private readonly IDepartmentApiClient _departmentApiClient;
        private readonly IPositionApiClient _positionApiClient;
        private readonly IEmployeeAssignmentApiClient _assignmentApiClient;

        public TransferModel(
            IEmployeeApiClient employeeApiClient,
            IDepartmentApiClient departmentApiClient,
            IPositionApiClient positionApiClient,
            IEmployeeAssignmentApiClient assignmentApiClient)
        {
            _employeeApiClient = employeeApiClient;
            _departmentApiClient = departmentApiClient;
            _positionApiClient = positionApiClient;
            _assignmentApiClient = assignmentApiClient;
        }

        [BindProperty]
        public TransferEmployeeViewModel Input { get; set; } = new();

        public EmployeeDetailModel Employee { get; set; } = new();
        public EmployeeAssignmentItemViewModel? CurrentAssignment { get; set; }
        public List<EmployeeAssignmentItemViewModel> AssignmentHistory { get; set; } = new();

        public List<SelectListItem> DepartmentOptions { get; set; } = new();
        public List<SelectListItem> PositionOptions { get; set; } = new();

        public bool HasCurrentAssignment { get; set; }

        public async Task<IActionResult> OnGetAsync(int employeeId)
        {
            ViewData["ActivePage"] = "EmployeeAssignments";

            if (employeeId <= 0)
            {
                return RedirectToPage("/Employees/Index");
            }

            var empRes = await _employeeApiClient.GetEmployeeDetailAsync(employeeId);
            if (empRes.StatusCode == 401)
            {
                HttpContext.Session.Remove("JWToken");
                return RedirectToPage("/Account/Login");
            }

            if (!empRes.Success || empRes.Data == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân viên yêu cầu.";
                return RedirectToPage("/Employees/Index");
            }

            Employee = empRes.Data;

            // Load assignment history
            var historyRes = await _assignmentApiClient.GetAssignmentHistoryAsync(employeeId);
            if (historyRes.Success && historyRes.Data != null)
            {
                AssignmentHistory = historyRes.Data;
                CurrentAssignment = AssignmentHistory.FirstOrDefault(a => a.IsCurrent);
            }

            HasCurrentAssignment = CurrentAssignment != null;

            if (!HasCurrentAssignment)
            {
                TempData["ErrorMessage"] = $"Nhân viên {Employee.FullName} chưa có phân công hiện tại. Vui lòng gán phân công đầu tiên.";
            }
            else
            {
                // Default StartDate for new assignment = today or current assignment start date + 1 day
                var today = DateOnly.FromDateTime(DateTime.Today);
                Input.StartDate = today > CurrentAssignment!.StartDate ? today : CurrentAssignment.StartDate.AddDays(1);
            }

            await LoadDropdownOptionsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int employeeId)
        {
            ViewData["ActivePage"] = "EmployeeAssignments";

            if (employeeId <= 0)
            {
                return RedirectToPage("/Employees/Index");
            }

            if (!ModelState.IsValid)
            {
                await ReloadPageContextAsync(employeeId);
                return Page();
            }

            var response = await _assignmentApiClient.TransferEmployeeAsync(employeeId, Input);

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
                ModelState.AddModelError(string.Empty, response.Message ?? "Điều chuyển nhân viên thất bại.");
                await ReloadPageContextAsync(employeeId);
                return Page();
            }

            TempData["SuccessMessage"] = $"Đã điều chuyển nhân viên {Employee.FullName} sang phòng ban / chức vụ mới thành công!";
            return RedirectToPage("/Employees/Details", new { id = employeeId });
        }

        private async Task ReloadPageContextAsync(int employeeId)
        {
            var empRes = await _employeeApiClient.GetEmployeeDetailAsync(employeeId);
            if (empRes.Success && empRes.Data != null)
            {
                Employee = empRes.Data;
            }

            var historyRes = await _assignmentApiClient.GetAssignmentHistoryAsync(employeeId);
            if (historyRes.Success && historyRes.Data != null)
            {
                AssignmentHistory = historyRes.Data;
                CurrentAssignment = AssignmentHistory.FirstOrDefault(a => a.IsCurrent);
            }

            HasCurrentAssignment = CurrentAssignment != null;
            await LoadDropdownOptionsAsync();
        }

        private async Task LoadDropdownOptionsAsync()
        {
            var deptRes = await _departmentApiClient.GetDepartmentsAsync(isActive: true);
            if (deptRes.Success && deptRes.Data != null)
            {
                DepartmentOptions = deptRes.Data
                    .Select(d => new SelectListItem
                    {
                        Value = d.DepartmentId.ToString(),
                        Text = d.DepartmentName
                    })
                    .ToList();
            }
            DepartmentOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn phòng ban mới --" });

            var posRes = await _positionApiClient.GetPositionsAsync(isActive: true);
            if (posRes.Success && posRes.Data != null)
            {
                PositionOptions = posRes.Data
                    .Select(p => new SelectListItem
                    {
                        Value = p.PositionId.ToString(),
                        Text = p.PositionName
                    })
                    .ToList();
            }
            PositionOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn chức vụ mới --" });
        }
    }
}
