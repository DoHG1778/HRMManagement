namespace HRM.Razor.Models.ViewModels.AttendanceAdjustments
{
    public class AdjustableAttendanceModel
    {
        public int AttendanceId { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal? WorkingHours { get; set; }
        public string? Status { get; set; }
        public bool HasPendingAdjustment { get; set; }
    }
}
