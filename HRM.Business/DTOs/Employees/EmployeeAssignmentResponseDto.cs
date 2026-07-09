namespace HRM.Business.DTOs.Employees
{
    public class EmployeeAssignmentResponseDto
    {
        public int AssignmentId { get; set; }

        public int EmployeeId { get; set; }

        public string EmployeeCode { get; set; } = string.Empty;

        public string EmployeeName { get; set; } = string.Empty;

        public int DepartmentId { get; set; }

        public string DepartmentName { get; set; } = string.Empty;

        public int PositionId { get; set; }

        public string PositionName { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public string? Note { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
