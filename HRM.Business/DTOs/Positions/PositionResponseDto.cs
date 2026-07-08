namespace HRM.Business.DTOs.Positions
{
    public class PositionResponseDto
    {
        public int PositionId { get; set; }

        public string PositionName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; }

        public int EmployeeCount { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }
}