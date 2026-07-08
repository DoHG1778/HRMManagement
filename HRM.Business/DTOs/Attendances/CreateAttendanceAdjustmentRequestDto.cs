using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Attendances
{
    public class CreateAttendanceAdjustmentRequestDto
    {
        [Required(ErrorMessage = "AttendanceId is required.")]
        public int AttendanceId { get; set; }

        public DateTime? RequestedCheckInTime { get; set; }

        public DateTime? RequestedCheckOutTime { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}