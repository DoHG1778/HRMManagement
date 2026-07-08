using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Attendances
{
    public class ApproveAttendanceAdjustmentRequestDto
    {
        [Required(ErrorMessage = "IsApproved is required.")]
        public bool IsApproved { get; set; }

        [MaxLength(500)]
        public string? RejectionReason { get; set; }
    }
}