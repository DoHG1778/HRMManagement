namespace HRM.Business.DTOs.Attendances
{
    public class CheckInResponseDto
    {
        public int AttendanceId { get; set; }

        public int EmployeeId { get; set; }

        public DateOnly AttendanceDate { get; set; }

        public DateTime? CheckInTime { get; set; }

        public string Status { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
    }
}