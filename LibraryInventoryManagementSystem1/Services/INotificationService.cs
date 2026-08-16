using LibraryInventoryManagementSystem1.Models;

namespace LibraryInventoryManagementSystem1.Services
{
    public interface INotificationService
    {
        // Send to one specific student
        Task SendToStudentAsync(int studentId, string title, string message, string type = "Announcement");

        // Broadcast to every student
        Task SendToAllStudentsAsync(string title, string message, string type = "Announcement");

        // Used by the student dashboard notifications list
        Task<List<Notification>> GetForStudentAsync(int studentId);

        // Used by the "click to view details" action (scoped to the owning student)
        Task<Notification?> GetByIdForStudentAsync(int id, int studentId);

        Task<int> GetUnreadCountAsync(int studentId);

        Task MarkAsReadAsync(int id, int studentId);
    }
}
