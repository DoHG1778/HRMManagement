using HRM.Business.Common;
using HRM.Business.DTOs.Notifications;
using HRM.Business.Services.Interfaces;
using HRM.Repositories.UnitOfWork;

namespace HRM.Business.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PagedResult<NotificationResponseDto>>> GetMyNotificationsAsync(
            CurrentUser currentUser,
            NotificationFilterDto filter)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<NotificationResponseDto>> GetNotificationDetailAsync(
            CurrentUser currentUser,
            int notificationId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(
            CurrentUser currentUser,
            int notificationId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> DeleteNotificationAsync(
            CurrentUser currentUser,
            int notificationId)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<NotificationResponseDto>> SendNotificationAsync(
            CurrentUser currentUser,
            SendNotificationRequestDto request)
        {
            throw new NotImplementedException();
        }

        public async Task<ApiResponse<bool>> SendSystemNotificationAsync(
            string title,
            string content,
            string notificationType,
            List<int> userIds)
        {
            throw new NotImplementedException();
        }
    }
}