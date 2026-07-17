namespace HRM.Business.DTOs.Dashboard
{
    public class DashboardResponseDto
    {
        public int TotalEmployees { get; set; }

        public int ActiveEmployees { get; set; }

        public int TotalDepartments { get; set; }

        public int TotalKpis { get; set; }

        public int ActiveKpis { get; set; }

        public int TotalAssignments { get; set; }

        public int CompletedAssignments { get; set; }

        public int InProgressAssignments { get; set; }

        public int OverdueAssignments { get; set; }
    }
}