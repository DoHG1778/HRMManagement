using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Overtimes
{
    public class UpdateOvertimeRequestDto
    {
        [Required(ErrorMessage = "OT date is required.")]
        public DateOnly Otdate { get; set; }

        [Required(ErrorMessage = "Start time is required.")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required.")]
        public DateTime EndTime { get; set; }

        [Required(ErrorMessage = "Reason is required.")]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }
}