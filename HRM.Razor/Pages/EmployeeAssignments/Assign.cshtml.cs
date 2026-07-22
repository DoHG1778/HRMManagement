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
    public class AssignModel : PageModel
    {
        private readonly IEmployeeApiClient _employeeApiClient;
        private readonly IDepartmentApiClient _departmentApiClient;
        private readonly IPositionApiClient _positionApiClient;
        private readonly IEmployeeAssignmentApiClient _assignmentApiClient;

        public AssignModel(
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
        public AssignEmployeeViewModel Input { get; set; } = new();

        public List<SelectListItem> EmployeeOptions { get; set; } = new();
        public List<SelectListItem> DepartmentOptions { get; set; } = new();
        public List<SelectListItem> PositionOptions { get; set; } = new();

        public EmployeeDetailModel? SelectedEmployee { get; set; }
        public bool SelectedEmployeeHasAssignment { get; set; }

        public async Task<IActionResult> OnGetAsync(int? employeeId)
        {
            ViewData["ActivePage"] = "EmployeeAssignments";

            await LoadDropdownDataAsync();

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                Input.EmployeeId = employeeId.Value;
                await LoadSelectedEmployeeDetailsAsync(employeeId.Value);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ViewData["ActivePage"] = "EmployeeAssignments";

            if (!ModelState.IsValid)
            {
                await LoadDropdownDataAsync();
                if (Input.EmployeeId > 0)
                {
                    await LoadSelectedEmployeeDetailsAsync(Input.EmployeeId);
                }
                return Page();
            }

            var response = await _assignmentApiClient.AssignEmployeeAsync(Input.EmployeeId, Input);

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
                var errorMsg = response.Message ?? "Gán nhân viên thất bại.";

                // Nếu backend báo nhân viên đã có current assignment -> hướng dẫn chuyển sang UC12
                if (errorMsg.Contains("already has a current assignment", StringComparison.OrdinalIgnoreCase))
                {
                    TempData["ErrorMessage"] = "Nhân viên đã có phân công công việc hiện tại. Vui lòng sử dụng tính năng Điều chuyển nhân viên.";
                    return RedirectToPage("/EmployeeAssignments/Transfer", new { employeeId = Input.EmployeeId });
                }

                ModelState.AddModelError(string.Empty, errorMsg);
                await LoadDropdownDataAsync();
                await LoadSelectedEmployeeDetailsAsync(Input.EmployeeId);
                return Page();
            }

            TempData["SuccessMessage"] = "Đã gán nhân viên vào phòng ban và chức vụ thành công!";
            return RedirectToPage("/Employees/Details", new { id = Input.EmployeeId });
        }

        private async Task LoadDropdownDataAsync()
        {
            // Load ACTIVE Employees
            var empRes = await _employeeApiClient.GetEmployeesAsync(pageSize: 100, status: "ACTIVE");
            if (empRes.Success && empRes.Data?.Items != null)
            {
                EmployeeOptions = empRes.Data.Items
                    .Select(e => new SelectListItem
                    {
                        Value = e.EmployeeId.ToString(),
                        Text = $"{e.EmployeeCode} - {e.FullName} ({e.Email})"
                    })
                    .ToList();
            }
            EmployeeOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn nhân viên cần gán --" });

            // Load ACTIVE Departments
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
            DepartmentOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn phòng ban --" });

            // Load ACTIVE Positions
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
            PositionOptions.Insert(0, new SelectListItem { Value = "", Text = "-- Chọn chức vụ --" });
        }

        private async Task LoadSelectedEmployeeDetailsAsync(int employeeId)
        {
            var empRes = await _employeeApiClient.GetEmployeeDetailAsync(employeeId);
            if (empRes.Success && empRes.Data != null)
            {
                SelectedEmployee = empRes.Data;
                SelectedEmployeeHasAssignment = empRes.Data.DepartmentId.HasValue || empRes.Data.PositionId.HasValue;
            }
        }
    }
}
