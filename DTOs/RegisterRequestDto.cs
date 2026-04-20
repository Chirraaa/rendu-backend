using System.ComponentModel.DataAnnotations;

namespace KanbanApp.API.DTOs
{
    public class RegisterRequestDto
    {
        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 1)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(254, MinimumLength = 3)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(128, MinimumLength = 8)]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one number, and one special character.")]
        public string Password { get; set; } = string.Empty;
    }
}
