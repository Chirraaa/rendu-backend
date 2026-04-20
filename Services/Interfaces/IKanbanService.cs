using KanbanApp.API.DTOs;

namespace KanbanApp.API.Services.Interfaces
{
    public interface IKanbanService
    {
        Task<List<KanbanDto>> GetUserKanbansAsync(int userId);
        Task<KanbanDto> CreateKanbanAsync(int userId, CreateKanbanDto dto);
        Task<bool> DeleteOrLeaveKanbanAsync(int kanbanId, int userId);
    }
}