using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Kpis
{
    public class AssignKpiRequestDto
    {
        [Required(ErrorMessage = "KPI id is required.")]
        public int Kpiid { get; set; }

        [Required(ErrorMessage = "EmployeeId is required.")]
        public int EmployeeId { get; set; }

        public decimal? TargetValue { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }
    }
}