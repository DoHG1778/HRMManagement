using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Payrolls
{
    public class GeneratePayrollRequestDto
    {
        [Range(1, 12, ErrorMessage = "Payroll month must be from 1 to 12.")]
        public int PayrollMonth { get; set; }

        [Range(2000, 2100, ErrorMessage = "Payroll year must be valid.")]
        public int PayrollYear { get; set; }

        public int? DepartmentId { get; set; }

        public int? EmployeeId { get; set; }
    }
}