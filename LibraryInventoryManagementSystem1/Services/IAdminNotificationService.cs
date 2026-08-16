using LibraryInventoryManagementSystem1.Models;

namespace LibraryInventoryManagementSystem1.Services
{
    public interface IAdminNotificationService
    {
        Task NotifyAsync(
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
            string? priority = "Normal");

        Task<List<AdminNotification>> GetRecentAsync(int take = 20);
        Task<List<AdminNotification>> GetAllAsync();
        Task<int> GetUnreadCountAsync();
        Task MarkAsReadAsync(int id);
        Task MarkAllAsReadAsync();
        Task ArchiveAsync(int id);
    }
}