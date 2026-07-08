using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Overtimes
{
    public class ApproveOvertimeRequestDto
    {
        [Required(ErrorMessage = "IsApproved is required.")]
        public bool IsApproved { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }
}