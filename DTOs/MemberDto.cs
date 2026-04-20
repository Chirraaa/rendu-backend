using System.ComponentModel.DataAnnotations;
using KanbanApp.API.Models;

namespace KanbanApp.API.DTOs
{
    public class MemberDto
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }

    public class InviteMemberDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateMemberRoleDto
    {
        [Required]
        [AllowedValues(MemberRoles.Admin, MemberRoles.Member,
            ErrorMessage = "Role must be 'Admin' or 'Member'.")]
        public string Role { get; set; } = string.Empty;
    }
}