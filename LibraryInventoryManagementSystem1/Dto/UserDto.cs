using System.ComponentModel.DataAnnotations;

namespace LibraryInventoryManagementSystem1.Dto
{
    public class UserDto
    {
        [Required(ErrorMessage = "Please enter a username.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter a password.")]
        public string Password { get; set; } = string.Empty;

        public string? Role { get; set; }

        public bool RememberMe { get; set; }
    }
}