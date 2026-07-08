using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Departments
{
    public class UpdateDepartmentRequestDto
    {
        [MaxLength(150)]
        public string? DepartmentName { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public int? ManagerEmployeeId { get; set; }

        public bool? IsActive { get; set; }
    }
}