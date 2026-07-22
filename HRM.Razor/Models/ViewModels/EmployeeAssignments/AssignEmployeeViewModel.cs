using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels.EmployeeAssignments
{
    public class AssignEmployeeViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn nhân viên.")]
        [Display(Name = "Nhân viên")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn phòng ban.")]
        [Display(Name = "Phòng ban")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn chức vụ.")]
        [Display(Name = "Chức vụ")]
        public int PositionId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày bắt đầu.")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày bắt đầu")]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [MaxLength(500, ErrorMessage = "Ghi chú không được quá 500 ký tự.")]
        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }
    }
}
