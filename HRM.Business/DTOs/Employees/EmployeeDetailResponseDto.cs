namespace HRM.Business.DTOs.Employees
{
    public class EmployeeDetailResponseDto
    {
        public int EmployeeId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public string Gender { get; set; } = string.Empty;

        public DateOnly? DateOfBirth { get; set; }

        public string? Phone { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? Cccd { get; set; }

        public DateOnly HireDate { get; set; }

        public string EmploymentStatus { get; set; } = string.Empty;

        public string? AvatarUrl { get; set; }

        public int? UserId { get; set; }

        public string? Username { get; set; }

        public int? ManagerId { get; set; }

        public string? ManagerName { get; set; }

        public int? DepartmentId { get; set; }

        public string? DepartmentName { get; set; }

        public int? PositionId { get; set; }

        public string? PositionName { get; set; }

        public decimal? CurrentSalary { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}