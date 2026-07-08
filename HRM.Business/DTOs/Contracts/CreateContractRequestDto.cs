using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Contracts
{
    public class CreateContractRequestDto
    {
        [Required(ErrorMessage = "EmployeeId is required.")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Contract type is required.")]
        [MaxLength(100)]
        public string ContractType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required.")]
        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [Required(ErrorMessage = "Salary is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than 0.")]
        public decimal Salary { get; set; }

        [MaxLength(500)]
        public string? ContractFileUrl { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}