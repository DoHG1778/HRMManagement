using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Notifications
{
    public class MarkNotificationReadRequestDto
    {
        [Required(ErrorMessage = "NotificationId is required.")]
        public int NotificationId { get; set; }
    }
}