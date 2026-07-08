namespace HRM.Business.DTOs.Dashboards
{
    public class DashboardSummaryDto
    {
        public int TotalEmployees { get; set; }

        public int ActiveEmployees { get; set; }

        public int OnLeaveEmployees { get; set; }

        public int ResignedEmployees { get; set; }

        public int TerminatedEmployees { get; set; }

        public int TotalDepartments { get; set; }

        public int PendingLeaveRequests { get; set; }

        public int PendingOvertimeRequests { get; set; }

        public int PendingAttendanceAdjustments { get; set; }

        public int CurrentMonthPayrolls { get; set; }

        public int ConfirmedPayrolls { get; set; }

        public int DraftPayrolls { get; set; }

        public int AssignedKpis { get; set; }

        public int EvaluatedKpis { get; set; }

        public int UnreadNotifications { get; set; }
    }
}