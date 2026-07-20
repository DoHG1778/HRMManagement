using System;

namespace HRM.Business.DTOs.Attendances
{
    public class AdjustableAttendanceDto
    {
        public int AttendanceId { get; set; }
        public DateOnly AttendanceDate { get; set; }
        public DateTime? CheckInTime { get; set; }
        public DateTime? CheckOutTime { get; set; }
        public decimal WorkingHours { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool HasPendingAdjustment { get; set; }
    }
}
