using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryInventoryManagementSystem1.Models
{
    public class Fine
    {
        [Key]
        public int FineId { get; set; }
        public int Id { get; set; }

        public int IssueId { get; set; }

        [ForeignKey("IssueId")]
        public BookIssue? BookIssue { get; set; }

        public decimal Amount { get; set; }

        // "Pending", "Paid"
        public string PaymentStatus { get; set; } = "Pending";

        public DateTime? PaymentDate { get; set; }
        //public object Id { get; internal set; }


    }
}