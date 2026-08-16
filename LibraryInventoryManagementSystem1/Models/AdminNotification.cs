using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryInventoryManagementSystem1.Models
{
    public class AdminNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(300)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        [Required]
        public AdminNotificationType Type { get; set; }

        [MaxLength(50)]
        public string? Icon { get; set; }

        [MaxLength(20)]
        public string? Priority { get; set; }

        public int? RelatedStudentId { get; set; }
        [ForeignKey(nameof(RelatedStudentId))]
        public Student? RelatedStudent { get; set; }

        public int? RelatedBookId { get; set; }
        [ForeignKey(nameof(RelatedBookId))]
        public Book? RelatedBook { get; set; }

        public int? RelatedBookIssueId { get; set; }
        [ForeignKey(nameof(RelatedBookIssueId))]
        public BookIssue? RelatedBookIssue { get; set; }

        public int? RelatedFineId { get; set; }
        [ForeignKey(nameof(RelatedFineId))]
        public Fine? RelatedFine { get; set; }

        public int? RelatedReservationId { get; set; }
        [ForeignKey(nameof(RelatedReservationId))]
        public Reservation? RelatedReservation { get; set; }

        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        public bool IsArchived { get; set; } = false;

        [MaxLength(100)]
        public string? TriggeredByUser { get; set; }

        [MaxLength(20)]
        public string? TriggeredByRole { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

    public enum AdminNotificationType
    {
        BookReserved,
        BookIssued,
        BookReturned,
        BookOverdue,
        FineAdded,
        FinePaid,
        NewStudentRegistered,
        LowStockAlert,
        System
    }
}