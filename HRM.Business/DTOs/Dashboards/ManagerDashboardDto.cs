namespace HRM.Business.DTOs.Dashboards
{
    public class ManagerDashboardDto
    {
        public int ManagerEmployeeId { get; set; }

        public string ManagerName { get; set; } = string.Empty;

        public int? DepartmentId { get; set; }

        public string? DepartmentName { get; set; }

        public int TotalEmployeesInDepartment { get; set; }

        public int ActiveEmployeesInDepartment { get; set; }

        public int PendingLeaveRequests { get; set; }

        public int PendingOvertimeRequests { get; set; }

        public int PendingAttendanceAdjustments { get; set; }

        public int AssignedKpis { get; set; }

        public int EvaluatedKpis { get; set; }
    }
}