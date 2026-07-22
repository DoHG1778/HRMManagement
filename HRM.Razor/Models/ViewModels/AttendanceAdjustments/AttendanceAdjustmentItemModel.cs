namespace HRM.Razor.Models.ViewModels.AttendanceAdjustments
{
    public class AttendanceAdjustmentItemModel
    {
        public int AdjustmentId { get; set; }
        public int AttendanceId { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public DateTime? OriginalCheckInTime { get; set; }
        public DateTime? OriginalCheckOutTime { get; set; }
        public decimal? OriginalWorkingHours { get; set; }
        public DateTime? RequestedCheckInTime { get; set; }
        public DateTime? RequestedCheckOutTime { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "PENDING";
        public int RequestedBy { get; set; }
        public string? RequestedByName { get; set; }
        public string? EmployeeCode { get; set; }
        public string? DepartmentName { get; set; }
        public string? PositionName { get; set; }
        public int? ApprovedBy { get; set; }
        public string? ApprovedByName { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
