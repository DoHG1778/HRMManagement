namespace HRM.Business.DTOs.Payrolls
{
    public class PayrollReportDto
    {
        public int PayrollMonth { get; set; }

        public int PayrollYear { get; set; }

        public int TotalEmployees { get; set; }

        public decimal TotalBaseSalary { get; set; }

        public decimal TotalAllowance { get; set; }

        public decimal TotalBonus { get; set; }

        public decimal TotalOvertime { get; set; }

        public decimal TotalDeduction { get; set; }

        public decimal TotalGrossSalary { get; set; }

        public decimal TotalNetSalary { get; set; }

        public List<PayrollResponseDto> Payrolls { get; set; } = new();
    }
}