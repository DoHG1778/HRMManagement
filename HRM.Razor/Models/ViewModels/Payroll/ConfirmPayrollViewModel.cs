using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels.Payroll
{
    public class ConfirmPayrollViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn tháng tính lương.")]
        [Range(1, 12, ErrorMessage = "Tháng tính lương phải từ 1 đến 12.")]
        [Display(Name = "Tháng")]
        public int PayrollMonth { get; set; } = DateTime.Today.Month;

        [Required(ErrorMessage = "Vui lòng chọn năm tính lương.")]
        [Range(2000, 2100, ErrorMessage = "Năm tính lương không hợp lệ.")]
        [Display(Name = "Năm")]
        public int PayrollYear { get; set; } = DateTime.Today.Year;

        [Display(Name = "Phòng ban")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Nhân viên cụ thể")]
        public int? EmployeeId { get; set; }
    }
}
