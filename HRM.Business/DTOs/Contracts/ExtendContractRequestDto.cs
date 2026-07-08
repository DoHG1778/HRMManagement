using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Contracts
{
    public class ExtendContractRequestDto
    {
        [Required(ErrorMessage = "New end date is required.")]
        public DateOnly NewEndDate { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }
    }
}