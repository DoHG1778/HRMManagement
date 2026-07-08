namespace HRM.Business.DTOs.Payrolls
{
    public class PayrollResponseDto
    {
        public int PayrollId { get; set; }

        public int EmployeeId { get; set; }

        public string? EmployeeCode { get; set; }

        public string? EmployeeName { get; set; }

        public int PayrollMonth { get; set; }

        public int PayrollYear { get; set; }

        public decimal BaseSalary { get; set; }

        public decimal TotalAllowance { get; set; }

        public decimal TotalBonus { get; set; }

        public decimal TotalOvertime { get; set; }

        public decimal TotalDeduction { get; set; }

        public decimal GrossSalary { get; set; }

        public decimal NetSalary { get; set; }

        public string Status { get; set; } = string.Empty;

        public int? GeneratedByUserId { get; set; }

        public string? GeneratedByUsername { get; set; }

        public int? ConfirmedByUserId { get; set; }

        public string? ConfirmedByUsername { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ConfirmedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<PayrollDetailResponseDto> PayrollDetails { get; set; } = new();
    }
}