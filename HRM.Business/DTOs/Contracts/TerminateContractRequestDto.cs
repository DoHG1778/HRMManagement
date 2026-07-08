using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Contracts
{
    public class TerminateContractRequestDto
    {
        [Required(ErrorMessage = "Termination date is required.")]
        public DateOnly TerminationDate { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public bool UpdateEmployeeStatus { get; set; } = true;

        [MaxLength(30)]
        public string? NewEmployeeStatus { get; set; }
    }
}