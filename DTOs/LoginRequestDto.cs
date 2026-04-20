using System.ComponentModel.DataAnnotations;

namespace KanbanApp.API.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; } = false;
    }
}
