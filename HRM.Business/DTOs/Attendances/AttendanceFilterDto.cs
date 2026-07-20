namespace HRM.Business.DTOs.Attendances
{
    public class AttendanceFilterDto
    {
        public int? DepartmentId { get; set; }

        public int? EmployeeId { get; set; }

        public string? Keyword { get; set; }

        // Khôi phục lại tên cũ để không làm lỗi UC khác
        public DateOnly? FromDate { get; set; }

        public DateOnly? ToDate { get; set; }

        public int? Month { get; set; }

        public int? Year { get; set; }

        public string? Status { get; set; }

        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 10;
    }
}