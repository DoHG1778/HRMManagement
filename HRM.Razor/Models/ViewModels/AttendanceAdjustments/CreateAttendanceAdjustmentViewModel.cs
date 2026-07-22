using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels.AttendanceAdjustments
{
    public class CreateAttendanceAdjustmentViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn ngày chấm công cần điều chỉnh.")]
        [Display(Name = "Ngày chấm công")]
        public int AttendanceId { get; set; }

        [Display(Name = "Giờ vào đề xuất")]
        public DateTime? RequestedCheckInTime { get; set; }

        [Display(Name = "Giờ ra đề xuất")]
        public DateTime? RequestedCheckOutTime { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do điều chỉnh.")]
        [StringLength(255, ErrorMessage = "Lý do không được vượt quá 255 ký tự.")]
        [Display(Name = "Lý do điều chỉnh")]
        public string Reason { get; set; } = string.Empty;
    }
}
