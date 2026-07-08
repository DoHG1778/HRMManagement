namespace HRM.Business.DTOs.Employees
{
    public class EmployeeFilterDto
    {
        public string? Keyword { get; set; }

        public int? DepartmentId { get; set; }

        public int? PositionId { get; set; }

        public string? EmploymentStatus { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}