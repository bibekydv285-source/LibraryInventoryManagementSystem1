namespace LibraryInventoryManagementSystem1.Dto
{
    public class IssueBookDto
    {
        public int BookId { get; set; }
        public int StudentId { get; set; }
        public DateTime DueDate { get; set; }
    }
}