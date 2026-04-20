namespace KanbanApp.API.Models
{
    public class Column
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Order { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int KanbanId { get; set; }
        public Kanban Kanban { get; set; } = null!;

        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}