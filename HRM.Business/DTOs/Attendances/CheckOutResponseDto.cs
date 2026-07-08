namespace HRM.Business.DTOs.Attendances
{
    public class CheckOutResponseDto
    {
        public int AttendanceId { get; set; }

        public int EmployeeId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        public DateTime? CheckInTime { get; set; }

        public DateTime? CheckOutTime { get; set; }

        public decimal WorkingHours { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}