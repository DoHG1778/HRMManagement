namespace HRM.Razor.Models.ViewModels
{
    public class SendNotificationViewModel
    {
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string NotificationType { get; set; } = "GENERAL";

        public string TargetType { get; set; } = "USER";

        public List<int> UserIds { get; set; } = new();

        public List<int> EmployeeIds { get; set; } = new();

        public int? DepartmentId { get; set; }
    }
}