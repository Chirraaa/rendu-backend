using KanbanApp.API.Models;

namespace KanbanApp.API.Repositories.Interfaces
{
    public interface IKanbanRepository
    {
        Task<List<Kanban>> GetUserKanbansAsync(int userId);
        Task<Kanban?> GetKanbanByIdAsync(int kanbanId);
        Task<KanbanMember?> GetMembershipAsync(int kanbanId, int userId);
        Task<Kanban> CreateKanbanAsync(Kanban kanban, KanbanMember creator);
        Task DeleteKanbanAsync(int kanbanId);
        Task RemoveMemberAndUnassignTicketsAsync(int kanbanId, int userId);
        Task SaveChangesAsync();
    }
}