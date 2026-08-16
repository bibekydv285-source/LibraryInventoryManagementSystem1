using LibraryInventoryManagementSystem1.Data;
using LibraryInventoryManagementSystem1.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryInventoryManagementSystem1.Services
{
    public class AdminNotificationService : IAdminNotificationService
    {
        private readonly AppDbContext _db;

        public AdminNotificationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task NotifyAsync(
            string title,
            string message,
            AdminNotificationType type,
            int? studentId = null,
            int? bookId = null,
            int? bookIssueId = null,
            int? fineId = null,
            int? reservationId = null,
            string? triggeredByUser = null,
            string? triggeredByRole = null,
            string? priority = "Normal")
        {
            var notification = new AdminNotification
            {
                Title = title,
                Message = message,
                Type = type,
                RelatedStudentId = studentId,
                RelatedBookId = bookId,
                RelatedBookIssueId = bookIssueId,
                RelatedFineId = fineId,
                RelatedReservationId = reservationId,
                TriggeredByUser = triggeredByUser,
                TriggeredByRole = triggeredByRole,
                Priority = priority,
                CreatedAt = DateTime.Now
            };

            _db.AdminNotifications.Add(notification);
            await _db.SaveChangesAsync();
        }

        public async Task<List<AdminNotification>> GetRecentAsync(int take = 20)
        {
            return await _db.AdminNotifications
                .Where(n => !n.IsArchived)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<List<AdminNotification>> GetAllAsync()
        {
            return await _db.AdminNotifications
                .Where(n => !n.IsArchived)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _db.AdminNotifications
                .CountAsync(n => !n.IsRead && !n.IsArchived);
        }

        public async Task MarkAsReadAsync(int id)
        {
            var notification = await _db.AdminNotifications.FindAsync(id);
            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                notification.ReadAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync()
        {
            var unread = await _db.AdminNotifications
                .Where(n => !n.IsRead && !n.IsArchived)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.Now;
            }

            await _db.SaveChangesAsync();
        }

        public async Task ArchiveAsync(int id)
        {
            var notification = await _db.AdminNotifications.FindAsync(id);
            if (notification != null)
            {
                notification.IsArchived = true;
                await _db.SaveChangesAsync();
            }
        }
    }
}