namespace KanbanApp.API.DTOs
{
    public class BoardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string CurrentUserRole { get; set; } = string.Empty;
        public int CreatedByUserId { get; set; }
        public double TotalHours { get; set; }
        public List<ColumnDto> Columns { get; set; } = new();
        public List<MemberDto> Members { get; set; } = new();
    }
}