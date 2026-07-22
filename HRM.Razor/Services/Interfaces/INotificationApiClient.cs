using HRM.Razor.Models;
using HRM.Razor.Models.ViewModels;

namespace HRM.Razor.Services.Interfaces
{
    public interface INotificationApiClient
    {
        Task<ApiResponse<
            HRM.Razor.Models.PagedResultModel<NotificationViewModel>>>
            GetMyNotificationsAsync(
                bool? isRead = null,
                string? notificationType = null,
                int pageNumber = 1,
                int pageSize = 10);

        Task<ApiResponse<NotificationViewModel>>
            GetNotificationDetailAsync(
                int notificationId);

        Task<ApiResponse<bool>>
            MarkAsReadAsync(
                int notificationId);

        Task<ApiResponse<bool>>
            DeleteNotificationAsync(
                int notificationId);

        Task<ApiResponse<NotificationViewModel>>
            SendNotificationAsync(
                SendNotificationViewModel model);
    }
}