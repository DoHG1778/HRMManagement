using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels
{
    public class MyProfileViewModel
    {
        public EmployeeDetailModel Profile { get; set; } = new();

        public UpdateMyProfileViewModel Input { get; set; } = new();

        public bool HasLinkedEmployee { get; set; } = true;
    }

    public class UpdateMyProfileViewModel
    {
        [MaxLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự.")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [MaxLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự.")]
        [Display(Name = "Địa chỉ hiện tại")]
        public string? Address { get; set; }

        [MaxLength(500, ErrorMessage = "Đường dẫn ảnh đại diện không quá 500 ký tự.")]
        [Display(Name = "Đường dẫn Ảnh đại diện")]
        public string? AvatarUrl { get; set; }
    }
}
