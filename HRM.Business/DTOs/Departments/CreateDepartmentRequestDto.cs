using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Departments
{
    public class CreateDepartmentRequestDto
    {
        [Required(ErrorMessage = "Department name is required.")]
        [MaxLength(150)]
        public string DepartmentName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ManagerEmployeeId { get; set; }
    }
}