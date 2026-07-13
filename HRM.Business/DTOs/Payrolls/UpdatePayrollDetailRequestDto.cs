using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Payrolls
{
    public class UpdatePayrollDetailRequestDto
    {
        [Required(ErrorMessage = "Items are required.")]
        public List<PayrollDetailItemDto> Items { get; set; } = new();
    }

    public class PayrollDetailItemDto
    {
        [Required(ErrorMessage = "Item type is required.")]
        [MaxLength(50)]
        public string ItemType { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Amount cannot be negative.")]
        public decimal Amount { get; set; }

        [MaxLength(50)]
        public string? SourceType { get; set; }

        public int? SourceId { get; set; }
    }
}