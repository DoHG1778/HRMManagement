using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Kpis
{
    public class EvaluateKpiRequestDto
    {
        [Range(0, 100, ErrorMessage = "Manager score must be from 0 to 100.")]
        public decimal ManagerScore { get; set; }

        [MaxLength(1000)]
        public string? ManagerComment { get; set; }
    }
}