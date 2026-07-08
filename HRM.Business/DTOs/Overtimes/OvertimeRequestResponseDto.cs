namespace HRM.Business.DTOs.Overtimes
{
    public class OvertimeRequestResponseDto
    {
        public int Otid { get; set; }

        public int EmployeeId { get; set; }

        public string? EmployeeCode { get; set; }

        public string? EmployeeName { get; set; }

        public DateOnly Otdate { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public decimal TotalHours { get; set; }

        public string? Reason { get; set; }

        public string Status { get; set; } = string.Empty;

        public int? ApprovedBy { get; set; }

        public string? ApprovedByName { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? RejectionReason { get; set; }

        public bool IsTransferredToPayroll { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}