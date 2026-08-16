using System.ComponentModel.DataAnnotations;

namespace LibraryInventoryManagementSystem1.Models
{
    public class BookIssue
    {
        [Key]
        public int IssueId { get; set; }

        public int BookId { get; set; }
        public Book? Book { get; set; }

        public int StudentId { get; set; }
        public Student? Student { get; set; }

        public DateTime IssueDate { get; set; } = DateTime.Now;
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }

        // "Issued", "Returned", "Overdue"
        public string Status { get; set; } = "Issued";
    }
}