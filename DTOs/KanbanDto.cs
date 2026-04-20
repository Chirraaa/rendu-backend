using System.ComponentModel.DataAnnotations;

namespace KanbanApp.API.DTOs
{
    public class KanbanDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int MemberCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateKanbanDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }
}