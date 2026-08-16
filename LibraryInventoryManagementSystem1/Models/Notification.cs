using System;

namespace LibraryInventoryManagementSystem1.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        // DueReminder, Overdue, Reservation, Announcement
        public string Type { get; set; } = "Announcement";

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsRead { get; set; } = false;
    }
}
