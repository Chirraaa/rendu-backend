namespace KanbanApp.API.Models
{
    public class Ticket
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public double TimeSpent { get; set; } = 0;
        public int Order { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int ColumnId { get; set; }
        public Column Column { get; set; } = null!;

        public int? AssignedToUserId { get; set; }
        public User? AssignedTo { get; set; }

        public ICollection<TicketMovementHistory> MovementHistory { get; set; } = new List<TicketMovementHistory>();

    }
}