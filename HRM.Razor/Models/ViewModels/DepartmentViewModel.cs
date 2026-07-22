using System.ComponentModel.DataAnnotations;

namespace HRM.Razor.Models.ViewModels
{
    public class DepartmentModel
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ManagerEmployeeId { get; set; }
        public string? ManagerName { get; set; }
        public bool IsActive { get; set; }
        public int EmployeeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class CreateDepartmentViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Tên phòng ban.")]
        [MaxLength(150, ErrorMessage = "Tên phòng ban không được quá 150 ký tự.")]
        [Display(Name = "Tên phòng ban")]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(500, ErrorMessage = "Mô tả không được quá 500 ký tự.")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Trưởng phòng ban")]
        public int? ManagerEmployeeId { get; set; }
    }

    public class EditDepartmentViewModel : CreateDepartmentViewModel
    {
        public int DepartmentId { get; set; }

        [Display(Name = "Trạng thái hoạt động")]
        public bool IsActive { get; set; } = true;
    }
}
