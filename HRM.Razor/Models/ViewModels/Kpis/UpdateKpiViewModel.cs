using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels.Kpis
{
    public class UpdateKpiViewModel
    {
        [MaxLength(200)]
        public string? Kpiname { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0.01, 100,
            ErrorMessage = "Weight must be between 0.01 and 100.")]
        public decimal? Weight { get; set; }

        public bool? IsActive { get; set; }
    }
}