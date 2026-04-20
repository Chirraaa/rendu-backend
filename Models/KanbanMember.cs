namespace KanbanApp.API.Models
{
    public class KanbanMember
    {
        public int Id { get; set; }
        public string Role { get; set; } = "Member"; // "Admin" or "Member"
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        public int KanbanId { get; set; }
        public Kanban Kanban { get; set; } = null!;

        public int UserId { get; set; }
        public User User { get; set; } = null!;
    }
}