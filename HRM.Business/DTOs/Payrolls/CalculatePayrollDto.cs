using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Payrolls
{
    public class CalculatePayrollDto
    {
        [Range(1, 12, ErrorMessage = "Payroll month must be from 1 to 12.")]
        public int PayrollMonth { get; set; }

        [Range(2000, 2100, ErrorMessage = "Payroll year must be valid.")]
        public int PayrollYear { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Default allowance cannot be negative.")]
        public decimal DefaultAllowance { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Default bonus cannot be negative.")]
        public decimal DefaultBonus { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Default deduction cannot be negative.")]
        public decimal DefaultDeduction { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Overtime rate cannot be negative.")]
        public decimal OvertimeRate { get; set; }
    }
}
