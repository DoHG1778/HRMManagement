using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels
{
    public class CreateEmployeeViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Mã nhân viên.")]
        [MaxLength(20, ErrorMessage = "Mã nhân viên không được quá 20 ký tự.")]
        [Display(Name = "Mã nhân viên")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Họ và tên.")]
        [MaxLength(150, ErrorMessage = "Họ và tên không được quá 150 ký tự.")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn Giới tính.")]
        [MaxLength(10)]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; } = "Nam";

        [Display(Name = "Ngày sinh")]
        public DateOnly? DateOfBirth { get; set; }

        [MaxLength(20, ErrorMessage = "Số điện thoại không quá 20 ký tự.")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email.")]
        [EmailAddress(ErrorMessage = "Định dạng Email không hợp lệ.")]
        [MaxLength(100, ErrorMessage = "Email không quá 100 ký tự.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [MaxLength(255, ErrorMessage = "Địa chỉ không quá 255 ký tự.")]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [MaxLength(20, ErrorMessage = "Số CCCD không quá 20 ký tự.")]
        [Display(Name = "Số CCCD / CMND")]
        public string? Cccd { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Ngày vào làm.")]
        [Display(Name = "Ngày vào làm")]
        public DateOnly HireDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Display(Name = "Tài khoản liên kết")]
        public int? UserId { get; set; }

        [Display(Name = "Quản lý trực tiếp")]
        public int? ManagerId { get; set; }

        [MaxLength(500)]
        [Display(Name = "Đường dẫn Ảnh đại diện")]
        public string? AvatarUrl { get; set; }

        [Display(Name = "Vai trò hệ thống")]
        public string SelectedRole { get; set; } = "Employee";
    }
}
