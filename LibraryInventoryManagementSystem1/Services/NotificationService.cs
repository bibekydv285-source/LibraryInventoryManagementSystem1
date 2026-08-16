using LibraryInventoryManagementSystem1.Data;
using LibraryInventoryManagementSystem1.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryInventoryManagementSystem1.Services
{
    public class NotificationService : INotificationService
    {
        private readonly AppDbContext _db;

        public NotificationService(AppDbContext db)
        {
            _db = db;
        }

        public async Task SendToStudentAsync(int studentId, string title, string message, string type = "Announcement")
        {
            var notification = new Notification
            {
                StudentId = studentId,
                Title = title,
                Message = message,
                Type = type,
                CreatedDate = DateTime.Now,
                IsRead = false
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }

        public async Task SendToAllStudentsAsync(string title, string message, string type = "Announcement")
        {
            var studentIds = await _db.Students.Select(s => s.Id).ToListAsync();

            var notifications = studentIds.Select(id => new Notification
            {
                StudentId = id,
                Title = title,
                Message = message,
                Type = type,
                CreatedDate = DateTime.Now,
                IsRead = false
            });

            _db.Notifications.AddRange(notifications);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetForStudentAsync(int studentId)
        {
            return await _db.Notifications
                .Where(n => n.StudentId == studentId)
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdForStudentAsync(int id, int studentId)
        {
            return await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.StudentId == studentId);
        }

        public async Task<int> GetUnreadCountAsync(int studentId)
        {
            return await _db.Notifications
                .CountAsync(n => n.StudentId == studentId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int id, int studentId)
        {
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.StudentId == studentId);

            if (notification != null && !notification.IsRead)
            {
                notification.IsRead = true;
                await _db.SaveChangesAsync();
            }
        }
    }
}
