using HRM.Business.Common;
using HRM.Business.DTOs.Notifications;

namespace HRM.Business.Services.Interfaces
{
    public interface INotificationService
    {
        Task<ApiResponse<PagedResult<NotificationResponseDto>>> GetMyNotificationsAsync(
            CurrentUser currentUser,
            NotificationFilterDto filter);

        Task<ApiResponse<NotificationResponseDto>> GetNotificationDetailAsync(
            CurrentUser currentUser,
            int notificationId);

        Task<ApiResponse<bool>> MarkAsReadAsync(
            CurrentUser currentUser,
            int notificationId);

        Task<ApiResponse<bool>> DeleteNotificationAsync(
            CurrentUser currentUser,
            int notificationId);

        Task<ApiResponse<NotificationResponseDto>> SendNotificationAsync(
            CurrentUser currentUser,
            SendNotificationRequestDto request);

        Task<ApiResponse<bool>> SendSystemNotificationAsync(
            string title,
            string content,
            string notificationType,
            List<int> userIds);
    }
}