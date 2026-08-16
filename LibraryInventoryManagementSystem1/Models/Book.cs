using System.ComponentModel.DataAnnotations;

namespace LibraryInventoryManagementSystem1.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Author { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public int TotalQty { get; set; }

        public int AvailableQty { get; set; }

        public string ISBN { get; set; } = string.Empty;

        // Consider validating ISBN format if needed (ISBN-10/ISBN-13)
    }
}