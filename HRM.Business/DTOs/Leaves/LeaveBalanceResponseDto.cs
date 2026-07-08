namespace HRM.Business.DTOs.Leaves
{
    public class LeaveBalanceResponseDto
    {
        public int LeaveBalanceId { get; set; }

        public int EmployeeId { get; set; }

        public string? EmployeeCode { get; set; }

        public string? EmployeeName { get; set; }

        public int LeaveTypeId { get; set; }

        public string? LeaveTypeName { get; set; }

        public int Year { get; set; }

        public int TotalDays { get; set; }

        public int UsedDays { get; set; }

        public int RemainingDays { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}