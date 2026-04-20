using System.ComponentModel.DataAnnotations;

namespace KanbanApp.API.DTOs
{
    public class ColumnDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public int TicketCount { get; set; }
        public double TotalHours { get; set; }
        public List<TicketDto> Tickets { get; set; } = new();
    }

    public class CreateColumnDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }

    public class UpdateColumnDto
    {
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
    }

    public class ReorderColumnsDto
    {
        [Required]
        [MinLength(1)]
        public List<int> ColumnIds { get; set; } = new();
    }
}