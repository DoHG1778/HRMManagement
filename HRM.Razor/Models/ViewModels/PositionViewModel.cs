using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels
{
    public class PositionModel
    {
        public int PositionId { get; set; }
        public string PositionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public int EmployeeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreatePositionViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Tên chức vụ.")]
        [MaxLength(150, ErrorMessage = "Tên chức vụ không được quá 150 ký tự.")]
        [Display(Name = "Tên chức vụ")]
        public string PositionName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự.")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
    }

    public class EditPositionViewModel : CreatePositionViewModel
    {
        public int PositionId { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}
