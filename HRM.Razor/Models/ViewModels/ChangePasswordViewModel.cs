using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu hiện tại")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới.")]
        [MinLength(8, ErrorMessage = "Mật khẩu mới phải có tối thiểu 8 ký tự.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu mới")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu mới.")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Xác nhận mật khẩu không khớp với mật khẩu mới.")]
        [Display(Name = "Xác nhận mật khẩu mới")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Đăng xuất khỏi các thiết bị khác")]
        public bool SignOutOtherDevices { get; set; }
    }
}
