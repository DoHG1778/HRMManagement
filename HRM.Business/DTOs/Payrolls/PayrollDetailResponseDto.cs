namespace HRM.Business.DTOs.Payrolls
{
    public class PayrollDetailResponseDto
    {
        public int PayrollDetailId { get; set; }

        public int PayrollId { get; set; }

        public string ItemType { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Amount { get; set; }

        public string? SourceType { get; set; }

        public int? SourceId { get; set; }

        public bool IsManual { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}