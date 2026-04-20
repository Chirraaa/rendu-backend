using KanbanApp.API.DTOs;

namespace KanbanApp.API.Services.Interfaces
{
    public interface IBoardService
    {
        Task<BoardDto?> GetBoardAsync(int kanbanId, int userId);

        // Columns
        Task<ColumnDto?> AddColumnAsync(int kanbanId, int userId, CreateColumnDto dto);
        Task<bool> UpdateColumnAsync(int kanbanId, int userId, int columnId, UpdateColumnDto dto);
        Task<bool> DeleteColumnAsync(int kanbanId, int userId, int columnId);
        Task<bool> ReorderColumnsAsync(int kanbanId, int userId, ReorderColumnsDto dto);

        // Tickets
        Task<TicketDto?> AddTicketAsync(int kanbanId, int userId, CreateTicketDto dto);
        Task<bool> UpdateTicketAsync(int kanbanId, int userId, int ticketId, UpdateTicketDto dto);
        Task<bool> DeleteTicketAsync(int kanbanId, int userId, int ticketId);
        Task<bool> MoveTicketAsync(int kanbanId, int userId, int ticketId, MoveTicketDto dto);
        Task<bool> ReorderTicketsAsync(int kanbanId, int userId, int columnId, ReorderTicketsDto dto);
        Task<List<TicketHistoryDto>?> GetTicketHistoryAsync(int kanbanId, int userId, int ticketId);

        // Members
        Task<MemberDto?> InviteMemberAsync(int kanbanId, int userId, InviteMemberDto dto);
        Task<bool> UpdateMemberRoleAsync(int kanbanId, int userId, int targetUserId, UpdateMemberRoleDto dto);
        Task<bool> RemoveMemberAsync(int kanbanId, int userId, int targetUserId);
    }
}