namespace HRM.Razor.Models.ViewModels
{
    public class NotificationViewModel
    {
        public int NotificationId { get; set; }

        public int RecipientId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string NotificationType { get; set; } = string.Empty;

        public int? CreatedByUserId { get; set; }

        public string? CreatedByUsername { get; set; }

        public bool IsRead { get; set; }

        public DateTime? ReadAt { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeletedAt { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}