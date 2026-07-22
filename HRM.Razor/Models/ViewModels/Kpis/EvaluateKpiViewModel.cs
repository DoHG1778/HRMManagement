using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels.Kpis
{
    public class EvaluateKpiViewModel
    {
        [Range(0, 100,
            ErrorMessage = "Manager score must be between 0 and 100.")]
        public decimal ManagerScore { get; set; }

        [MaxLength(1000)]
        public string? ManagerComment { get; set; }
    }
}