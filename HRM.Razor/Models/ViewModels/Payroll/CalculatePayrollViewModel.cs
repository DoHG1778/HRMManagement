using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels.Payroll
{
    public class CalculatePayrollViewModel
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

        [Display(Name = "Nhân viên")]
        public int? EmployeeId { get; set; }

        [Range(0, 100000000, ErrorMessage = "Phụ cấp mặc định phải không âm.")]
        [Display(Name = "Phụ cấp mặc định")]
        public decimal DefaultAllowance { get; set; } = 0;

        [Range(0, 100000000, ErrorMessage = "Thưởng mặc định phải không âm.")]
        [Display(Name = "Thưởng mặc định")]
        public decimal DefaultBonus { get; set; } = 0;

        [Range(0, 100000000, ErrorMessage = "Khấu trừ mặc định phải không âm.")]
        [Display(Name = "Khấu trừ mặc định")]
        public decimal DefaultDeduction { get; set; } = 0;

        [Range(0.1, 5.0, ErrorMessage = "Hệ số tăng ca phải từ 0.1 đến 5.0.")]
        [Display(Name = "Hệ số tăng ca (OT)")]
        public decimal OvertimeCoefficient { get; set; } = 1.5m;

        [Range(1, 31, ErrorMessage = "Số ngày công chuẩn phải từ 1 đến 31.")]
        [Display(Name = "Số ngày công chuẩn")]
        public int StandardWorkingDays { get; set; } = 24;

        [Range(1, 24, ErrorMessage = "Số giờ làm việc / ngày phải từ 1 đến 24.")]
        [Display(Name = "Số giờ làm việc / ngày")]
        public int StandardWorkingHoursPerDay { get; set; } = 8;
    }
}
