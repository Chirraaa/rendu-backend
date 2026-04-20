using KanbanApp.API.Data;
using KanbanApp.API.Models;
using KanbanApp.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KanbanApp.API.Repositories
{
    public class KanbanRepository : IKanbanRepository
    {
        private readonly AppDbContext _context;

        public KanbanRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Kanban>> GetUserKanbansAsync(int userId) => await _context.Kanbans
        .Include(k => k.Members)
        .Where(k => k.Members.Any(m => m.UserId == userId))
        .ToListAsync();

        public async Task<Kanban?> GetKanbanByIdAsync(int kanbanId) => await _context.Kanbans
        .Include(k => k.Members)
        .ThenInclude(m => m.User)
        .Include(k => k.Columns)
        .ThenInclude(c => c.Tickets)
        .FirstOrDefaultAsync(k => k.Id == kanbanId);

        public async Task<KanbanMember?> GetMembershipAsync(int kanbanId, int userId) => await _context.KanbanMembers
        .FirstOrDefaultAsync(m => m.KanbanId == kanbanId && m.UserId == userId);

        public async Task<Kanban> CreateKanbanAsync(Kanban kanban, KanbanMember creator)
        {
            creator.Kanban = kanban;
            _context.Kanbans.Add(kanban);
            _context.KanbanMembers.Add(creator);
            await _context.SaveChangesAsync();
            return kanban;
        }

        public async Task DeleteKanbanAsync(int kanbanId)
        {
            await _context.Database.ExecuteSqlRawAsync(
                @"DELETE FROM ""Kanbans"" WHERE ""Id"" = {0}", kanbanId);
        }

        public async Task RemoveMemberAndUnassignTicketsAsync(int kanbanId, int userId)
        {
            await _context.Database.ExecuteSqlRawAsync(@"
                UPDATE ""Tickets"" SET ""AssignedToUserId"" = NULL
                WHERE ""AssignedToUserId"" = {0}
                AND ""ColumnId"" IN (
                    SELECT ""Id"" FROM ""Columns"" WHERE ""KanbanId"" = {1}
                )", userId, kanbanId);

            var membership = await GetMembershipAsync(kanbanId, userId);
            if (membership != null)
            {
                _context.KanbanMembers.Remove(membership);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}