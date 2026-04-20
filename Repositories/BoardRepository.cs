using KanbanApp.API.Data;
using KanbanApp.API.Models;
using KanbanApp.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KanbanApp.API.Repositories
{
    public class BoardRepository : IBoardRepository
    {
        private readonly AppDbContext _context;

        public BoardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Kanban?> GetBoardAsync(int kanbanId)
            => await _context.Kanbans
                .Include(k => k.Members).ThenInclude(m => m.User)
                .Include(k => k.Columns).ThenInclude(c => c.Tickets).ThenInclude(t => t.AssignedTo)
                .FirstOrDefaultAsync(k => k.Id == kanbanId);

        public async Task<KanbanMember?> GetMembershipAsync(int kanbanId, int userId)
            => await _context.KanbanMembers
                .FirstOrDefaultAsync(m => m.KanbanId == kanbanId && m.UserId == userId);

        public async Task<int?> GetKanbanCreatorIdAsync(int kanbanId)
            => await _context.Kanbans
                .Where(k => k.Id == kanbanId)
                .Select(k => (int?)k.CreatedByUserId)
                .FirstOrDefaultAsync();

        public async Task<Column?> GetColumnInKanbanAsync(int columnId, int kanbanId)
            => await _context.Columns
                .FirstOrDefaultAsync(c => c.Id == columnId && c.KanbanId == kanbanId);

        public async Task<List<Column>> GetColumnsByKanbanAsync(int kanbanId)
            => await _context.Columns
                .Where(c => c.KanbanId == kanbanId)
                .ToListAsync();

        public async Task AddColumnAsync(Column column)
            => await _context.Columns.AddAsync(column);

        public async Task<int> GetMaxColumnOrderAsync(int kanbanId)
            => await _context.Columns
                .Where(c => c.KanbanId == kanbanId)
                .MaxAsync(c => (int?)c.Order) ?? 0;

        public Task DeleteColumnAsync(Column column)
        {
            _context.Columns.Remove(column);
            return Task.CompletedTask;
        }

        public async Task<Ticket?> GetTicketAsync(int ticketId)
            => await _context.Tickets
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == ticketId);

        public async Task<Ticket?> GetTicketInKanbanAsync(int ticketId, int kanbanId)
            => await _context.Tickets
                .Include(t => t.AssignedTo)
                .FirstOrDefaultAsync(t => t.Id == ticketId && t.Column.KanbanId == kanbanId);

        public async Task<List<Ticket>> GetTicketsByColumnAsync(int columnId)
            => await _context.Tickets
                .Where(t => t.ColumnId == columnId)
                .ToListAsync();

        public async Task AddTicketAsync(Ticket ticket)
            => await _context.Tickets.AddAsync(ticket);

        public Task DeleteTicketAsync(Ticket ticket)
        {
            _context.Tickets.Remove(ticket);
            return Task.CompletedTask;
        }

        public async Task AddMovementHistoryAsync(TicketMovementHistory history)
            => await _context.TicketMovementHistories.AddAsync(history);

        public async Task<List<TicketMovementHistory>> GetTicketHistoryAsync(int ticketId)
            => await _context.TicketMovementHistories
                .Include(h => h.FromColumn)
                .Include(h => h.ToColumn)
                .Where(h => h.TicketId == ticketId)
                .OrderByDescending(h => h.MovedAt)
                .ToListAsync();

        public async Task<int> GetMaxTicketOrderAsync(int columnId)
            => await _context.Tickets
                .Where(t => t.ColumnId == columnId)
                .MaxAsync(t => (int?)t.Order) ?? 0;

        public async Task<User?> GetUserByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task AddMemberAsync(KanbanMember member)
            => await _context.KanbanMembers.AddAsync(member);

        public Task DeleteMemberAsync(KanbanMember member)
        {
            _context.KanbanMembers.Remove(member);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
            => await _context.SaveChangesAsync();
    }
}