using HRM.DataAccess.Contexts;
using HRM.Models.Entities;
using HRM.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HRM.Repositories.Implementations
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<NotificationRecipient>> GetNotificationsByUserAsync(
            int userId,
            bool? isRead,
            string? notificationType)
        {
            var query = _context.NotificationRecipients
                .Include(nr => nr.Notification)
                .Where(nr =>
                    nr.UserId == userId &&
                    !nr.IsDeleted);

            if (isRead.HasValue)
            {
                query = query.Where(nr => nr.IsRead == isRead.Value);
            }

            if (!string.IsNullOrWhiteSpace(notificationType))
            {
                query = query.Where(nr =>
                    nr.Notification.NotificationType == notificationType);
            }

            return await query
                .OrderByDescending(nr => nr.Notification.CreatedAt)
                .ToListAsync();
        }

        public async Task<NotificationRecipient?> GetNotificationRecipientAsync(
            int notificationId,
            int userId)
        {
            return await _context.NotificationRecipients
                .Include(nr => nr.Notification)
                .FirstOrDefaultAsync(nr =>
                    nr.NotificationId == notificationId &&
                    nr.UserId == userId &&
                    !nr.IsDeleted);
        }

        public async Task<bool> IsNotificationOwnedByUserAsync(
            int notificationId,
            int userId)
        {
            return await _context.NotificationRecipients
                .AnyAsync(nr =>
                    nr.NotificationId == notificationId &&
                    nr.UserId == userId &&
                    !nr.IsDeleted);
        }

        public async Task AddRecipientsAsync(IEnumerable<NotificationRecipient> recipients)
        {
            await _context.NotificationRecipients.AddRangeAsync(recipients);
        }

        public async Task MarkAsReadAsync(int notificationId, int userId)
        {
            var recipient = await _context.NotificationRecipients
                .FirstOrDefaultAsync(nr =>
                    nr.NotificationId == notificationId &&
                    nr.UserId == userId &&
                    !nr.IsDeleted);

            if (recipient == null)
            {
                return;
            }

            recipient.IsRead = true;
            recipient.ReadAt = DateTime.Now;
        }

        public async Task SoftDeleteAsync(int notificationId, int userId)
        {
            var recipient = await _context.NotificationRecipients
                .FirstOrDefaultAsync(nr =>
                    nr.NotificationId == notificationId &&
                    nr.UserId == userId &&
                    !nr.IsDeleted);

            if (recipient == null)
            {
                return;
            }

            recipient.IsDeleted = true;
            recipient.DeletedAt = DateTime.Now;
        }
    }
}