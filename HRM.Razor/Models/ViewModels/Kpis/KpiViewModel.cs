namespace HRM.Razor.Models.ViewModels.Kpis
{
    public class KpiViewModel
    {
        public int Kpiid { get; set; }

        public string Kpiname { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Weight { get; set; }

        public bool IsActive { get; set; }

        public int? CreatedByUserId { get; set; }

        public string? CreatedByUsername { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}