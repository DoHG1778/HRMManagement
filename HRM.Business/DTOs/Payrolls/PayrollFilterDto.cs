namespace HRM.Business.DTOs.Payrolls
{
    public class PayrollFilterDto
    {
        public int PayrollMonth { get; set; }

        public int PayrollYear { get; set; }

        public int? DepartmentId { get; set; }

        public int? EmployeeId { get; set; }

        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}