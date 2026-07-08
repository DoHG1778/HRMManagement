using System.ComponentModel.DataAnnotations;

namespace HRM.Business.DTOs.Notifications
{
    public class SendNotificationRequestDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Content is required.")]
        public string Content { get; set; } = string.Empty;

        [Required(ErrorMessage = "Notification type is required.")]
        [MaxLength(50)]
        public string NotificationType { get; set; } = "GENERAL";

        [MaxLength(30)]
        public string TargetType { get; set; } = "USER";

        public List<int> UserIds { get; set; } = new();

        public List<int> EmployeeIds { get; set; } = new();

        public int? DepartmentId { get; set; }
    }
}