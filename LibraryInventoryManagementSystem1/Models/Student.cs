using System.ComponentModel.DataAnnotations;

namespace LibraryInventoryManagementSystem1.Models
{
    public class Student
    {
        public int Id { get; set; }

        public string StudentCode { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? Course { get; set; }
        public int? Age { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}