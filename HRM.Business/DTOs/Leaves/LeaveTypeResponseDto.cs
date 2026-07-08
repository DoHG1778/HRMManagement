namespace HRM.Business.DTOs.Leaves
{
    public class LeaveTypeResponseDto
    {
        public int LeaveTypeId { get; set; }

        public string LeaveTypeName { get; set; } = string.Empty;

        public int MaxDaysPerYear { get; set; }

        public string? Description { get; set; }

        public bool IsActive { get; set; }
    }
}