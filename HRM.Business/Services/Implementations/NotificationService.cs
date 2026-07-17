using HRM.Business.Common;
using HRM.Business.DTOs.Notifications;
using HRM.Business.Services.Interfaces;
using HRM.Models.Entities;
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
            if (!currentUser.IsAuthenticated)
            {
                return ApiResponse<PagedResult<NotificationResponseDto>>
                    .Unauthorized();
            }

            var notifications = await _unitOfWork.Notifications
                .GetNotificationsByUserAsync(
                    currentUser.UserId,
                    filter.IsRead,
                    filter.NotificationType);
            var result = notifications
            .Select(x => new NotificationResponseDto
            {
                NotificationId = x.NotificationId,
                RecipientId = x.RecipientId,
                Title = x.Notification.Title,
                Content = x.Notification.Content,
                NotificationType = x.Notification.NotificationType,
                CreatedByUserId = x.Notification.CreatedByUserId,
                CreatedByUsername = x.Notification.CreatedByUser?.Username,
                IsRead = x.IsRead,
                ReadAt = x.ReadAt,
                IsDeleted = x.IsDeleted,
                DeletedAt = x.DeletedAt,
                CreatedAt = x.Notification.CreatedAt
            })
            .ToList();
            var totalItems = result.Count;

            var items = result
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var paged = PagedResult<NotificationResponseDto>.Create(
                items,
                filter.PageNumber,
                filter.PageSize,
                totalItems);

            return ApiResponse<PagedResult<NotificationResponseDto>>
                .Ok(paged);
        }

        public async Task<ApiResponse<NotificationResponseDto>> GetNotificationDetailAsync(
            CurrentUser currentUser,
            int notificationId)
        {
            if (!currentUser.IsAuthenticated)
            {
                return ApiResponse<NotificationResponseDto>
                    .Unauthorized();
            }

            var notificationRecipient =
                await _unitOfWork.Notifications
                    .GetNotificationRecipientAsync(
                        notificationId,
                        currentUser.UserId);

            if (notificationRecipient == null)
            {
                return ApiResponse<NotificationResponseDto>
                    .NotFound("Notification not found");
            }

            var result = new NotificationResponseDto
            {
                NotificationId = notificationRecipient.NotificationId,
                RecipientId = notificationRecipient.UserId,
                Title = notificationRecipient.Notification.Title,
                Content = notificationRecipient.Notification.Content,
                NotificationType = notificationRecipient.Notification.NotificationType,
                CreatedByUserId = notificationRecipient.Notification.CreatedByUserId,
                CreatedByUsername =
                    notificationRecipient.Notification.CreatedByUser?.Username,
                IsRead = notificationRecipient.IsRead,
                ReadAt = notificationRecipient.ReadAt,
                IsDeleted = notificationRecipient.IsDeleted,
                DeletedAt = notificationRecipient.DeletedAt,
                CreatedAt = notificationRecipient.Notification.CreatedAt
            };

            return ApiResponse<NotificationResponseDto>
                .Ok(result);
        }

        public async Task<ApiResponse<bool>> MarkAsReadAsync(
            CurrentUser currentUser,
            int notificationId)
        {
            if (!currentUser.IsAuthenticated)
            {
                return ApiResponse<bool>.Unauthorized();
            }


            var isOwned = await _unitOfWork.Notifications
                .IsNotificationOwnedByUserAsync(
                    notificationId,
                    currentUser.UserId);


            if (!isOwned)
            {
                return ApiResponse<bool>
                    .NotFound("Notification not found");
            }


            await _unitOfWork.Notifications
                .MarkAsReadAsync(
                    notificationId,
                    currentUser.UserId);


            await _unitOfWork.SaveChangesAsync();


            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<bool>> DeleteNotificationAsync(
            CurrentUser currentUser,
            int notificationId)
        {
            if (!currentUser.IsAuthenticated)
            {
                return ApiResponse<bool>.Unauthorized();
            }


            var isOwned = await _unitOfWork.Notifications
                .IsNotificationOwnedByUserAsync(
                    notificationId,
                    currentUser.UserId);


            if (!isOwned)
            {
                return ApiResponse<bool>
                    .NotFound("Notification not found");
            }


            await _unitOfWork.Notifications
                .SoftDeleteAsync(
                    notificationId,
                    currentUser.UserId);


            await _unitOfWork.SaveChangesAsync();


            return ApiResponse<bool>.Ok(true);
        }

        public async Task<ApiResponse<NotificationResponseDto>> SendNotificationAsync(
    CurrentUser currentUser,
    SendNotificationRequestDto request)
        {
            if (!currentUser.IsAuthenticated)
            {
                return ApiResponse<NotificationResponseDto>
                    .Unauthorized();
            }


            var notification = new Notification
            {
                Title = request.Title,
                Content = request.Content,
                NotificationType = request.NotificationType,
                CreatedByUserId = currentUser.UserId,
                CreatedAt = DateTime.Now
            };


            await _unitOfWork.Notifications
                .AddAsync(notification);


            await _unitOfWork.SaveChangesAsync();


            var recipients = request.UserIds
                .Select(userId => new NotificationRecipient
                {
                    NotificationId = notification.NotificationId,
                    UserId = userId,
                    IsRead = false,
                    IsDeleted = false
                })
                .ToList();


            await _unitOfWork.Notifications
                .AddRecipientsAsync(recipients);


            await _unitOfWork.SaveChangesAsync();


            var result = new NotificationResponseDto
            {
                NotificationId = notification.NotificationId,
                Title = notification.Title,
                Content = notification.Content,
                NotificationType = notification.NotificationType,
                CreatedByUserId = notification.CreatedByUserId,
                CreatedAt = notification.CreatedAt
            };


            return ApiResponse<NotificationResponseDto>
                .Ok(result);
        }

        public async Task<ApiResponse<bool>> SendSystemNotificationAsync(
    string title,
    string content,
    string notificationType,
    List<int> userIds)
        {
            if (userIds == null || userIds.Count == 0)
            {
                return ApiResponse<bool>
                    .Fail("No recipients found.");
            }


            var notification = new Notification
            {
                Title = title,
                Content = content,
                NotificationType = notificationType,
                CreatedByUserId = null,
                CreatedAt = DateTime.Now
            };


            await _unitOfWork.Notifications
                .AddAsync(notification);


            await _unitOfWork.SaveChangesAsync();


            var recipients = userIds
                .Select(userId => new NotificationRecipient
                {
                    NotificationId = notification.NotificationId,
                    UserId = userId,
                    IsRead = false,
                    IsDeleted = false
                })
                .ToList();


            await _unitOfWork.Notifications
                .AddRecipientsAsync(recipients);


            await _unitOfWork.SaveChangesAsync();


            return ApiResponse<bool>.Ok(true);
        }
    }
}