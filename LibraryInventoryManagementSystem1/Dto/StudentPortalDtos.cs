using System.ComponentModel.DataAnnotations;

namespace LibraryInventoryManagementSystem1.Dto
{
    public class ProfileDto
    {
        public int StudentId { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Course { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
    }

    public class ChangePasswordDto
    {
        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), MinLength(6)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password), Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class BookSearchDto
    {
        public string? Query { get; set; }
        public string? Category { get; set; }
        public System.Collections.Generic.List<LibraryInventoryManagementSystem1.Models.Book> Results { get; set; } = new();
    }
}
