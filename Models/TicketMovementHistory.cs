namespace KanbanApp.API.Models
{
    public class TicketMovementHistory
    {
        public int Id { get; set; }
        public DateTime MovedAt { get; set; } = DateTime.UtcNow;

        public int TicketId { get; set; }
        public Ticket Ticket { get; set; } = null!;

        public int? FromColumnId { get; set; }
        public Column? FromColumn { get; set; }

        public int? ToColumnId { get; set; }
        public Column? ToColumn { get; set; }
    }
}