using HRM.Models.Entities;

namespace HRM.Repositories.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<List<NotificationRecipient>> GetNotificationsByUserAsync(
            int userId,
            bool? isRead,
            string? notificationType);

        Task<NotificationRecipient?> GetNotificationRecipientAsync(
            int notificationId,
            int userId);

        Task<bool> IsNotificationOwnedByUserAsync(
            int notificationId,
            int userId);

        Task AddRecipientsAsync(IEnumerable<NotificationRecipient> recipients);

        Task MarkAsReadAsync(int notificationId, int userId);

        Task SoftDeleteAsync(int notificationId, int userId);
    }
}