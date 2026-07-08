namespace HRM.Business.DTOs.Users
{
    public class UserResponseDto
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime? LastLoginAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public List<string> Roles { get; set; } = new();

        public int? EmployeeId { get; set; }

        public string? EmployeeCode { get; set; }

        public string? FullName { get; set; }
    }
}