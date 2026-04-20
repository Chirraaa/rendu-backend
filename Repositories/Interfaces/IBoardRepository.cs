using KanbanApp.API.Models;

namespace KanbanApp.API.Repositories.Interfaces
{
    public interface IBoardRepository
    {
        // Board
        Task<Kanban?> GetBoardAsync(int kanbanId);
        Task<KanbanMember?> GetMembershipAsync(int kanbanId, int userId);
        Task<int?> GetKanbanCreatorIdAsync(int kanbanId);

        // Columns
        Task<Column?> GetColumnInKanbanAsync(int columnId, int kanbanId);
        Task<List<Column>> GetColumnsByKanbanAsync(int kanbanId);
        Task AddColumnAsync(Column column);
        Task DeleteColumnAsync(Column column);
        Task<int> GetMaxColumnOrderAsync(int kanbanId);

        // Tickets
        Task<Ticket?> GetTicketAsync(int ticketId);
        Task<Ticket?> GetTicketInKanbanAsync(int ticketId, int kanbanId);
        Task<List<Ticket>> GetTicketsByColumnAsync(int columnId);
        Task AddTicketAsync(Ticket ticket);
        Task DeleteTicketAsync(Ticket ticket);
        Task AddMovementHistoryAsync(TicketMovementHistory history);
        Task<List<TicketMovementHistory>> GetTicketHistoryAsync(int ticketId);
        Task<int> GetMaxTicketOrderAsync(int columnId);

        // Members
        Task<User?> GetUserByEmailAsync(string email);
        Task AddMemberAsync(KanbanMember member);
        Task DeleteMemberAsync(KanbanMember member);

        Task SaveChangesAsync();
    }
}