using System.ComponentModel.DataAnnotations;

namespace LibraryInventoryManagementSystem1.Dto
{
    public class SendNotificationDto
    {
        // Null when SendToAll is true
        public int? StudentId { get; set; }

        public bool SendToAll { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(120, ErrorMessage = "Title must be 120 characters or fewer.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required.")]
        [StringLength(1000, ErrorMessage = "Message must be 1000 characters or fewer.")]
        public string Message { get; set; } = string.Empty;

        [Required]
        public string Type { get; set; } = "Announcement";
    }
}
