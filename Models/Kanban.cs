namespace KanbanApp.API.Models
{
    public class Kanban
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CreatedByUserId { get; set; }
        public User CreatedBy { get; set; } = null!;

        public ICollection<KanbanMember> Members { get; set; } = new List<KanbanMember>();
        public ICollection<Column> Columns { get; set; } = new List<Column>();
    }
}