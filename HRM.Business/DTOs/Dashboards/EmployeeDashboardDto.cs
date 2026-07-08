namespace HRM.Business.DTOs.Dashboards
{
    public class EmployeeDashboardDto
    {
        public int EmployeeId { get; set; }

        public string EmployeeName { get; set; } = string.Empty;

        public int AttendanceDaysThisMonth { get; set; }

        public int LateDaysThisMonth { get; set; }

        public int PendingLeaveRequests { get; set; }

        public int ApprovedLeaveRequests { get; set; }

        public int PendingOvertimeRequests { get; set; }

        public int ApprovedOvertimeRequests { get; set; }

        public int AssignedKpis { get; set; }

        public int EvaluatedKpis { get; set; }

        public int UnreadNotifications { get; set; }

        public decimal? LatestNetSalary { get; set; }
    }
}