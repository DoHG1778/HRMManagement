namespace HRM.Razor.Models.ViewModels
{
    public class UserItemModel
    {
        public int UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string? Email { get; set; }

        public bool IsActive { get; set; }
    }
}