using KanbanApp.API.DTOs;
using KanbanApp.API.Models;
using KanbanApp.API.Repositories.Interfaces;
using KanbanApp.API.Services.Interfaces;

namespace KanbanApp.API.Services
{
    public class KanbanService : IKanbanService
    {
        private readonly IKanbanRepository _kanbanRepository;

        public KanbanService(IKanbanRepository kanbanRepository)
        {
            _kanbanRepository = kanbanRepository;
        }

        public async Task<List<KanbanDto>> GetUserKanbansAsync(int userId)
        {
            var kanbans = await _kanbanRepository.GetUserKanbansAsync(userId);

            return kanbans.Select(k => new KanbanDto
            {
                Id = k.Id,
                Name = k.Name,
                Role = k.Members.FirstOrDefault(m => m.UserId == userId)?.Role ?? MemberRoles.Member,
                MemberCount = k.Members.Count,
                CreatedAt = k.CreatedAt
            }).ToList();
        }

        public async Task<KanbanDto> CreateKanbanAsync(int userId, CreateKanbanDto dto)
        {
            var kanban = new Kanban
            {
                Name = dto.Name.Trim(),
                CreatedByUserId = userId
            };

            var creator = new KanbanMember
            {
                UserId = userId,
                Role = MemberRoles.Admin
            };

            await _kanbanRepository.CreateKanbanAsync(kanban, creator);

            return new KanbanDto
            {
                Id = kanban.Id,
                Name = kanban.Name,
                Role = MemberRoles.Admin,
                MemberCount = 1,
                CreatedAt = kanban.CreatedAt
            };
        }

        public async Task<bool> DeleteOrLeaveKanbanAsync(int kanbanId, int userId)
        {
            var membership = await _kanbanRepository.GetMembershipAsync(kanbanId, userId);
            if (membership == null) return false;

            if (membership.Role == MemberRoles.Admin)
                await _kanbanRepository.DeleteKanbanAsync(kanbanId);
            else
                await _kanbanRepository.RemoveMemberAndUnassignTicketsAsync(kanbanId, userId);

            return true;
        }
    }
}
