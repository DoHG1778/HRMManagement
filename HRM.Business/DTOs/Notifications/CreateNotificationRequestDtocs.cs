using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Notifications
{
    public class CreateNotificationRequestDto
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string NotificationType { get; set; } = string.Empty;

        [Required]
        public string TargetType { get; set; } = string.Empty;

        public int? TargetId { get; set; }
    }
}