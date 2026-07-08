namespace HRM.Business.DTOs.Kpis
{
    public class KpiAssignmentFilterDto
    {
        public int? EmployeeId { get; set; }

        public int? Kpiid { get; set; }

        public string? Status { get; set; }

        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}