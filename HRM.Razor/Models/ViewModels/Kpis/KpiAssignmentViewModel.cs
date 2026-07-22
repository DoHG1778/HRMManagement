namespace HRM.Razor.Models.ViewModels.Kpis
{
    public class KpiAssignmentViewModel
    {
        public int AssignmentId { get; set; }

        public int Kpiid { get; set; }

        public string? Kpiname { get; set; }

        public int EmployeeId { get; set; }

        public string? EmployeeCode { get; set; }

        public string? EmployeeName { get; set; }

        public int? AssignedBy { get; set; }

        public string? AssignedByName { get; set; }

        public decimal? TargetValue { get; set; }

        public decimal? ActualValue { get; set; }

        public decimal ProgressPercent { get; set; }

        public string? EmployeeComment { get; set; }

        public decimal? EmployeeSelfScore { get; set; }

        public decimal? ManagerScore { get; set; }

        public string? ManagerComment { get; set; }

        public int? ReviewedBy { get; set; }

        public string? ReviewedByName { get; set; }

        public DateTime? ReviewedAt { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}