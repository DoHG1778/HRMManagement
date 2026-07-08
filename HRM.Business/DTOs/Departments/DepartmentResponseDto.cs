namespace HRM.Business.DTOs.Departments
{
    public class DepartmentResponseDto
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
}