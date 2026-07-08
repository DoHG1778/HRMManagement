namespace HRM.Business.DTOs.Attendances
{
    public class AttendanceAdjustmentResponseDto
    {
        public int AdjustmentId { get; set; }

        public int AttendanceId { get; set; }

        public int RequestedBy { get; set; }

        public string? RequestedByName { get; set; }

        public DateTime? RequestedCheckInTime { get; set; }

        public DateTime? RequestedCheckOutTime { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public int? ApprovedBy { get; set; }

        public string? ApprovedByName { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}