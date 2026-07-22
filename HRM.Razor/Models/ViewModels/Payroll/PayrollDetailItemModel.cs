namespace HRM.Razor.Models.ViewModels.Payroll
{
    public class PayrollDetailItemModel
    {
        public int DetailId { get; set; }
        public int PayrollId { get; set; }
        public string ItemType { get; set; } = string.Empty; // ALLOWANCE, BONUS, DEDUCTION, OVERTIME, OTHER
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? SourceType { get; set; } // SYSTEM, OVERTIME, MANUAL
        public int? SourceId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
