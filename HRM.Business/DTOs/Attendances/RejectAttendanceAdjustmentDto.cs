using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Attendances
{
    public class RejectAttendanceAdjustmentDto
    {
        [Required(ErrorMessage = "Rejection reason is required.")]
        [MaxLength(500)]
        public string RejectionReason { get; set; } = string.Empty;
    }
}
