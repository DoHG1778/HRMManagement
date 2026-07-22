using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels.AttendanceAdjustments
{
    public class RejectAttendanceAdjustmentViewModel
    {
        [Required]
        public int AdjustmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do từ chối.")]
        [StringLength(255, ErrorMessage = "Lý do từ chối không được vượt quá 255 ký tự.")]
        [Display(Name = "Lý do từ chối")]
        public string RejectionReason { get; set; } = string.Empty;
    }
}
