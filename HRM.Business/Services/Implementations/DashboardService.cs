using HRM.Business.Common;
using HRM.Business.DTOs.Dashboards;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace HRM.Business.Services.Implementations
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<DashboardSummaryDto>> GetDashboardSummaryAsync(
            CurrentUser currentUser,
            int? month,
            int? year)
        {
            if (!currentUser.HasAnyRole("Admin", "HR", "Manager"))
            {
                return ApiResponse<DashboardSummaryDto>.Forbidden();
            }

            var employees = await _unitOfWork.Employees.GetAllAsync();
            var departments = await _unitOfWork.Departments.GetAllAsync();
            var payrolls = await _unitOfWork.Payrolls.GetAllAsync();
            var leaveRequests = await _unitOfWork.LeaveRequests.GetAllAsync();
            var overtimeRequests = await _unitOfWork.OvertimeRequests.GetAllAsync();
            var attendanceAdjustments =
                await _unitOfWork.AttendanceAdjustments.GetAllAsync();

            var kpis = await _unitOfWork.Kpis.GetAllAsync();

            var assignments = _unitOfWork.Kpis
                .Query()
                .SelectMany(k => k.Kpiassignments)
                .ToList();

            var dto = new DashboardSummaryDto
            {
                TotalEmployees = employees.Count,

                ActiveEmployees = employees.Count(e =>
                    e.EmploymentStatus == "ACTIVE"),

                OnLeaveEmployees = employees.Count(e =>
                    e.EmploymentStatus == "ON_LEAVE"),

                ResignedEmployees = employees.Count(e =>
                    e.EmploymentStatus == "RESIGNED"),

                TerminatedEmployees = employees.Count(e =>
                    e.EmploymentStatus == "TERMINATED"),

                TotalDepartments = departments.Count,

                PendingLeaveRequests = leaveRequests.Count(l =>
                    l.Status == "PENDING"),

                PendingOvertimeRequests = overtimeRequests.Count(o =>
                    o.Status == "PENDING"),

                PendingAttendanceAdjustments =
                    attendanceAdjustments.Count(a =>
                        a.Status == "PENDING"),

                CurrentMonthPayrolls = payrolls.Count,

                ConfirmedPayrolls = payrolls.Count(p =>
                    p.Status == "CONFIRMED"),

                DraftPayrolls = payrolls.Count(p =>
                    p.Status == "DRAFT"),

                AssignedKpis = assignments.Count,

                EvaluatedKpis = assignments.Count(a =>
                    a.ManagerScore != null)
            };

            return ApiResponse<DashboardSummaryDto>.Ok(dto);
        }

        public async Task<ApiResponse<EmployeeDashboardDto>> GetEmployeeDashboardAsync(
            CurrentUser currentUser,
            int? month,
            int? year)
        {
            if (currentUser.EmployeeId == null)
            {
                return ApiResponse<EmployeeDashboardDto>.Fail(
                    "Current user is not linked to employee.");
            }

            var employee = await _unitOfWork.Employees
                .GetByIdAsync(currentUser.EmployeeId.Value);

            if (employee == null)
            {
                return ApiResponse<EmployeeDashboardDto>.NotFound(
                    "Employee not found.");
            }

            var attendances = await _unitOfWork.Attendances.GetAllAsync();

            var leaveRequests = await _unitOfWork.LeaveRequests.GetAllAsync();

            var overtimeRequests = await _unitOfWork.OvertimeRequests.GetAllAsync();

            var payrolls = await _unitOfWork.Payrolls.GetAllAsync();

            var assignments = await _unitOfWork.Kpis
                .GetKpiAssignmentsByEmployeeAsync(employee.EmployeeId);

            var notifications = await _unitOfWork.Notifications
                .GetNotificationsByUserAsync(
                    currentUser.UserId,
                    null,
                    null);

            var currentMonth = month ?? DateTime.Now.Month;
            var currentYear = year ?? DateTime.Now.Year;

            var dto = new EmployeeDashboardDto
            {
                EmployeeId = employee.EmployeeId,

                EmployeeName = employee.FullName,

                AttendanceDaysThisMonth = attendances.Count(a =>
                    a.EmployeeId == employee.EmployeeId &&
                    a.AttendanceDate.Month == currentMonth &&
                    a.AttendanceDate.Year == currentYear),

                LateDaysThisMonth = attendances.Count(a =>
                    a.EmployeeId == employee.EmployeeId &&
                    a.AttendanceDate.Month == currentMonth &&
                    a.AttendanceDate.Year == currentYear &&
                    a.Status == "LATE"),

                PendingLeaveRequests = leaveRequests.Count(l =>
                    l.EmployeeId == employee.EmployeeId &&
                    l.Status == "PENDING"),

                ApprovedLeaveRequests = leaveRequests.Count(l =>
                    l.EmployeeId == employee.EmployeeId &&
                    l.Status == "APPROVED"),

                PendingOvertimeRequests = overtimeRequests.Count(o =>
                    o.EmployeeId == employee.EmployeeId &&
                    o.Status == "PENDING"),

                ApprovedOvertimeRequests = overtimeRequests.Count(o =>
                    o.EmployeeId == employee.EmployeeId &&
                    o.Status == "APPROVED"),

                AssignedKpis = assignments.Count,

                EvaluatedKpis = assignments.Count(a =>
                    a.ManagerScore != null),

                UnreadNotifications = notifications.Count(n =>
                    !n.IsRead),

                LatestNetSalary = payrolls
                    .Where(p => p.EmployeeId == employee.EmployeeId)
                    .OrderByDescending(p => p.PayrollYear)
                    .ThenByDescending(p => p.PayrollMonth)
                    .Select(p => (decimal?)p.NetSalary)
                    .FirstOrDefault()
            };

            return ApiResponse<EmployeeDashboardDto>.Ok(dto);
        }

        public async Task<ApiResponse<ManagerDashboardDto>> GetManagerDashboardAsync(
            CurrentUser currentUser,
            int? month,
            int? year)
        {
            if (!currentUser.IsManager())
            {
                return ApiResponse<ManagerDashboardDto>.Forbidden(
                    "Only Manager can view manager dashboard.");
            }

            if (currentUser.EmployeeId == null)
            {
                return ApiResponse<ManagerDashboardDto>.Fail(
                    "Current user is not linked to employee.");
            }

            var manager = await _unitOfWork.Employees
                .Query()
                .Include(e => e.EmployeeAssignments)
                    .ThenInclude(a => a.Department)
                .FirstOrDefaultAsync(e =>
        e.EmployeeId == currentUser.EmployeeId.Value);

            if (manager == null)
            {
                return ApiResponse<ManagerDashboardDto>.NotFound(
                    "Manager not found.");
            }

            var assignment = manager.EmployeeAssignments
                .FirstOrDefault(e => e.EndDate == null);

            if (assignment == null)
            {
                return ApiResponse<ManagerDashboardDto>.Fail(
                    "Manager has not been assigned to any department.");
            }

            var departmentId = assignment.DepartmentId;

            var departmentName = assignment.Department.DepartmentName;

            var employees = await _unitOfWork.Employees.GetAllAsync();
            var leaveRequests = await _unitOfWork.LeaveRequests.GetAllAsync();
            var overtimeRequests = await _unitOfWork.OvertimeRequests.GetAllAsync();
            var attendanceAdjustments =
                await _unitOfWork.AttendanceAdjustments.GetAllAsync();

            var assignments = await _unitOfWork.Kpis
                .GetKpiAssignmentsByManagerAsync(manager.EmployeeId);

            var employeeIds = employees
                .Where(e => e.EmployeeAssignments.Any(a =>
                    a.EndDate == null &&
                    a.DepartmentId == departmentId))
                .Select(e => e.EmployeeId)
                .ToList();

            var dto = new ManagerDashboardDto
            {
                ManagerEmployeeId = manager.EmployeeId,

                ManagerName = manager.FullName,

                DepartmentId = departmentId,

                DepartmentName = departmentName,

                TotalEmployeesInDepartment = employeeIds.Count,

                ActiveEmployeesInDepartment = employees.Count(e =>
                    employeeIds.Contains(e.EmployeeId) &&
                    e.EmploymentStatus == "ACTIVE"),

                PendingLeaveRequests = leaveRequests.Count(l =>
                    employeeIds.Contains(l.EmployeeId) &&
                    l.Status == "PENDING"),

                PendingOvertimeRequests = overtimeRequests.Count(o =>
                    employeeIds.Contains(o.EmployeeId) &&
                    o.Status == "PENDING"),

                PendingAttendanceAdjustments = attendanceAdjustments.Count(a =>
                    a.Attendance != null &&
                    employeeIds.Contains(a.Attendance.EmployeeId) &&
                    a.Status == "PENDING"),

                AssignedKpis = assignments.Count,

                EvaluatedKpis = assignments.Count(a =>
                    a.ManagerScore != null)
            };

            return ApiResponse<ManagerDashboardDto>.Ok(dto);
        }

        public async Task<ApiResponse<PayrollDashboardDto>> GetPayrollDashboardAsync(
            CurrentUser currentUser,
            int payrollMonth,
            int payrollYear)
        {
            if (!currentUser.IsPayroll())
            {
                return ApiResponse<PayrollDashboardDto>.Forbidden(
                    "Only Payroll can view payroll dashboard.");
            }

            var payrolls = await _unitOfWork.Payrolls.Query()
                .Where(p =>
                    p.PayrollMonth == payrollMonth &&
                    p.PayrollYear == payrollYear)
                .ToListAsync();

            var dto = new PayrollDashboardDto
            {
                PayrollMonth = payrollMonth,
                PayrollYear = payrollYear,
                TotalPayrolls = payrolls.Count,
                DraftPayrolls = payrolls.Count(p => p.Status == "DRAFT" && p.UpdatedAt == null),
                CalculatedPayrolls = payrolls.Count(p => p.Status == "DRAFT" && p.UpdatedAt != null),
                ConfirmedPayrolls = payrolls.Count(p => p.Status == "CONFIRMED"),
                PaidPayrolls = payrolls.Count(p => p.Status == "PAID"),
                TotalBaseSalary = payrolls.Sum(p => p.BaseSalary),
                TotalGrossSalary = payrolls.Sum(p => p.GrossSalary),
                TotalNetSalary = payrolls.Sum(p => p.NetSalary),
                TotalOvertime = payrolls.Sum(p => p.TotalOvertime),
                TotalDeduction = payrolls.Sum(p => p.TotalDeduction)
            };

            return ApiResponse<PayrollDashboardDto>.Ok(dto);
        }
    }
}