namespace KanbanApp.API.DTOs
{
    public class TicketHistoryDto
    {
        public int Id { get; set; }
        public string? FromColumnName { get; set; }
        public string? ToColumnName { get; set; }
        public DateTime MovedAt { get; set; }
    }
}
