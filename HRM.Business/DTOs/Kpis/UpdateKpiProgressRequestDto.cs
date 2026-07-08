using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Kpis
{
    public class UpdateKpiProgressRequestDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "Actual value must not be negative.")]
        public decimal? ActualValue { get; set; }

        [Range(0, 100, ErrorMessage = "Progress percent must be from 0 to 100.")]
        public decimal ProgressPercent { get; set; }

        [MaxLength(1000)]
        public string? EmployeeComment { get; set; }

        [Range(0, 100, ErrorMessage = "Employee self score must be from 0 to 100.")]
        public decimal? EmployeeSelfScore { get; set; }
    }
}