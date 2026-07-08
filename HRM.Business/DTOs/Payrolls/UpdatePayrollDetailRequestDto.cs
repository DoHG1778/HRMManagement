using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Payrolls
{
    public class UpdatePayrollDetailRequestDto
    {
        [Required(ErrorMessage = "Item type is required.")]
        [MaxLength(50)]
        public string ItemType { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        public decimal Amount { get; set; }

        [MaxLength(50)]
        public string? SourceType { get; set; }

        public int? SourceId { get; set; }
    }
}