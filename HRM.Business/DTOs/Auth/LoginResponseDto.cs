namespace HRM.Business.DTOs.Auth
{
    public class LoginResponseDto
    {
        public int UserId { get; set; }

        public int? EmployeeId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();

        public string Token { get; set; } = string.Empty;

        public DateTime ExpiredAt { get; set; }
    }
}