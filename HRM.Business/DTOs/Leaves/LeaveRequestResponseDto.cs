namespace HRM.Business.DTOs.Leaves
{
    public class LeaveRequestResponseDto
    {
        public int LeaveRequestId { get; set; }

        public int EmployeeId { get; set; }

        public string? EmployeeCode { get; set; }

        public string? EmployeeName { get; set; }

        public int LeaveTypeId { get; set; }

        public string? LeaveTypeName { get; set; }

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public int TotalDays { get; set; }

        public string? Reason { get; set; }

        public string Status { get; set; } = string.Empty;

        public int? ApprovedBy { get; set; }

        public string? ApprovedByName { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}