using System.ComponentModel.DataAnnotations;

namespace LibraryInventoryManagementSystem1.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public string Role { get; set; } = "User";

        public string? ResetCode { get; set; }
        public DateTime? ResetCodeExpiry { get; set; }

        public string? PhoneNumber { get; set; }
    }
}