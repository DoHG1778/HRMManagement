using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels.Kpis
{
    public class UpdateKpiProgressViewModel
    {
        [Range(0, double.MaxValue,
            ErrorMessage = "Actual value must not be negative.")]
        public decimal? ActualValue { get; set; }

        [Range(0, 100,
            ErrorMessage = "Progress must be between 0 and 100.")]
        public decimal ProgressPercent { get; set; }

        [MaxLength(1000)]
        public string? EmployeeComment { get; set; }

        [Range(0, 100,
            ErrorMessage = "Self score must be between 0 and 100.")]
        public decimal? EmployeeSelfScore { get; set; }
    }
}