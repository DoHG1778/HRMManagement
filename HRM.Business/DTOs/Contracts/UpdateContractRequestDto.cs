using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Contracts
{
    public class UpdateContractRequestDto
    {
        [MaxLength(100)]
        public string? ContractType { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than 0.")]
        public decimal? Salary { get; set; }

        [MaxLength(500)]
        public string? ContractFileUrl { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}