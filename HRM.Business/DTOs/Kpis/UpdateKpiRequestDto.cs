using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Kpis
{
    public class UpdateKpiRequestDto
    {
        [MaxLength(200)]
        public string? Kpiname { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0.01, 100, ErrorMessage = "KPI weight must be greater than 0 and less than or equal to 100.")]
        public decimal? Weight { get; set; }

        public bool? IsActive { get; set; }
    }
}