namespace HRM.Razor.Models.ViewModels
{
    public class LoginResponseModel
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
