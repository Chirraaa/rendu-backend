using System.ComponentModel.DataAnnotations;

namespace KanbanApp.API.DTOs
{
    public class TicketDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double TimeSpent { get; set; }
        public int ColumnId { get; set; }
        public int? AssignedToUserId { get; set; }
        public string AssignedToEmail { get; set; } = string.Empty;
        public string AssignedToName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTicketDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public double TimeSpent { get; set; } = 0;

        [Range(1, int.MaxValue)]
        public int ColumnId { get; set; }

        public int? AssignedToUserId { get; set; }
    }

    public class UpdateTicketDto
    {
        [Required]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public double TimeSpent { get; set; }

        [Range(1, int.MaxValue)]
        public int ColumnId { get; set; }

        public int? AssignedToUserId { get; set; }
    }

    public class MoveTicketDto
    {
        [Range(1, int.MaxValue)]
        public int TargetColumnId { get; set; }
    }

    public class ReorderTicketsDto
    {
        [Required]
        public List<int> TicketIds { get; set; } = new();
    }
}