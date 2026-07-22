using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels.EmployeeAssignments
{
    public class TransferEmployeeViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn phòng ban mới.")]
        [Display(Name = "Phòng ban mới")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chức vụ mới.")]
        [Display(Name = "Chức vụ mới")]
        public int PositionId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu mới.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu")]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [MaxLength(500, ErrorMessage = "Lý do điều chuyển không được quá 500 ký tự.")]
        [Display(Name = "Lý do điều chuyển")]
        public string? Note { get; set; }
    }
}
