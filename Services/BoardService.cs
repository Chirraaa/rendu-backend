using KanbanApp.API.DTOs;
using KanbanApp.API.Hubs;
using KanbanApp.API.Models;
using KanbanApp.API.Repositories.Interfaces;
using KanbanApp.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace KanbanApp.API.Services
{
    public class BoardService : IBoardService
    {
        private readonly IBoardRepository _repo;
        private readonly IHubContext<BoardHub> _hub;

        public BoardService(IBoardRepository repo, IHubContext<BoardHub> hub)
        {
            _repo = repo;
            _hub = hub;
        }

        private Task Broadcast(int kanbanId) =>
            _hub.Clients.Group($"board-{kanbanId}").SendAsync("BoardUpdated");

        public async Task<BoardDto?> GetBoardAsync(int kanbanId, int userId)
        {
            var board = await _repo.GetBoardAsync(kanbanId);
            if (board == null) return null;

            var membership = board.Members.FirstOrDefault(m => m.UserId == userId);
            if (membership == null) return null;

            return MapToDto(board, userId);
        }

        public async Task<ColumnDto?> AddColumnAsync(int kanbanId, int userId, CreateColumnDto dto)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership?.Role != MemberRoles.Admin) return null;

            var maxOrder = await _repo.GetMaxColumnOrderAsync(kanbanId);

            var column = new Column
            {
                Name = dto.Name.Trim(),
                KanbanId = kanbanId,
                Order = maxOrder + 1
            };

            await _repo.AddColumnAsync(column);
            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);

            return new ColumnDto
            {
                Id = column.Id,
                Name = column.Name,
                Order = column.Order,
                TicketCount = 0,
                TotalHours = 0,
                Tickets = new()
            };
        }

        public async Task<bool> UpdateColumnAsync(int kanbanId, int userId, int columnId, UpdateColumnDto dto)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership?.Role != MemberRoles.Admin) return false;

            var column = await _repo.GetColumnInKanbanAsync(columnId, kanbanId);
            if (column == null) return false;

            column.Name = dto.Name.Trim();
            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);
            return true;
        }

        public async Task<bool> DeleteColumnAsync(int kanbanId, int userId, int columnId)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership?.Role != MemberRoles.Admin) return false;

            var column = await _repo.GetColumnInKanbanAsync(columnId, kanbanId);
            if (column == null) return false;

            await _repo.DeleteColumnAsync(column);
            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);
            return true;
        }

        public async Task<bool> ReorderColumnsAsync(int kanbanId, int userId, ReorderColumnsDto dto)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership?.Role != MemberRoles.Admin) return false;

            var columns = await _repo.GetColumnsByKanbanAsync(kanbanId);
            var columnMap = columns.ToDictionary(c => c.Id);

            for (int i = 0; i < dto.ColumnIds.Count; i++)
                if (columnMap.TryGetValue(dto.ColumnIds[i], out var column))
                    column.Order = i + 1;

            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);
            return true;
        }

        public async Task<TicketDto?> AddTicketAsync(int kanbanId, int userId, CreateTicketDto dto)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership == null) return null;

            var column = await _repo.GetColumnInKanbanAsync(dto.ColumnId, kanbanId);
            if (column == null) return null;

            var assignedToUserId = userId;
            if (dto.AssignedToUserId.HasValue)
            {
                var assignedMember = await _repo.GetMembershipAsync(kanbanId, dto.AssignedToUserId.Value);
                if (assignedMember != null)
                    assignedToUserId = dto.AssignedToUserId.Value;
            }

            var maxOrder = await _repo.GetMaxTicketOrderAsync(dto.ColumnId);

            var ticket = new Ticket
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                TimeSpent = dto.TimeSpent,
                ColumnId = dto.ColumnId,
                AssignedToUserId = assignedToUserId,
                Order = maxOrder + 1
            };

            await _repo.AddTicketAsync(ticket);
            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);

            var reloaded = await _repo.GetTicketAsync(ticket.Id);
            return MapTicketToDto(reloaded!);
        }

        public async Task<bool> UpdateTicketAsync(int kanbanId, int userId, int ticketId, UpdateTicketDto dto)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership == null) return false;

            var ticket = await _repo.GetTicketInKanbanAsync(ticketId, kanbanId);
            if (ticket == null) return false;

            if (membership.Role != MemberRoles.Admin && ticket.AssignedToUserId != userId)
                return false;

            ticket.Title = dto.Title.Trim();
            ticket.Description = dto.Description?.Trim();
            ticket.TimeSpent = dto.TimeSpent;
            ticket.ColumnId = dto.ColumnId;
            if (dto.AssignedToUserId.HasValue)
                ticket.AssignedToUserId = dto.AssignedToUserId.Value;

            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);
            return true;
        }

        public async Task<bool> DeleteTicketAsync(int kanbanId, int userId, int ticketId)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership == null) return false;

            var ticket = await _repo.GetTicketInKanbanAsync(ticketId, kanbanId);
            if (ticket == null) return false;

            if (membership.Role != MemberRoles.Admin && ticket.AssignedToUserId != userId)
                return false;

            await _repo.DeleteTicketAsync(ticket);
            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);
            return true;
        }

        public async Task<bool> MoveTicketAsync(int kanbanId, int userId, int ticketId, MoveTicketDto dto)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership == null) return false;

            var ticket = await _repo.GetTicketInKanbanAsync(ticketId, kanbanId);
            if (ticket == null) return false;

            var targetColumn = await _repo.GetColumnInKanbanAsync(dto.TargetColumnId, kanbanId);
            if (targetColumn == null) return false;

            var history = new TicketMovementHistory
            {
                TicketId = ticketId,
                FromColumnId = ticket.ColumnId,
                ToColumnId = dto.TargetColumnId
            };

            var maxOrder = await _repo.GetMaxTicketOrderAsync(dto.TargetColumnId);
            await _repo.AddMovementHistoryAsync(history);
            ticket.ColumnId = dto.TargetColumnId;
            ticket.Order = maxOrder + 1;
            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);
            return true;
        }

        public async Task<bool> ReorderTicketsAsync(int kanbanId, int userId, int columnId, ReorderTicketsDto dto)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership == null) return false;

            var column = await _repo.GetColumnInKanbanAsync(columnId, kanbanId);
            if (column == null) return false;

            var tickets = await _repo.GetTicketsByColumnAsync(columnId);
            var ticketMap = tickets.ToDictionary(t => t.Id);

            for (int i = 0; i < dto.TicketIds.Count; i++)
                if (ticketMap.TryGetValue(dto.TicketIds[i], out var ticket))
                    ticket.Order = i + 1;

            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);
            return true;
        }

        public async Task<List<TicketHistoryDto>?> GetTicketHistoryAsync(int kanbanId, int userId, int ticketId)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership == null) return null;

            var ticket = await _repo.GetTicketInKanbanAsync(ticketId, kanbanId);
            if (ticket == null) return null;

            var history = await _repo.GetTicketHistoryAsync(ticketId);

            return history.Select(h => new TicketHistoryDto
            {
                Id = h.Id,
                FromColumnName = h.FromColumn?.Name,
                ToColumnName = h.ToColumn?.Name,
                MovedAt = h.MovedAt
            }).ToList();
        }

        public async Task<MemberDto?> InviteMemberAsync(int kanbanId, int userId, InviteMemberDto dto)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership?.Role != MemberRoles.Admin) return null;

            var user = await _repo.GetUserByEmailAsync(dto.Email.Trim().ToLowerInvariant());
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var existing = await _repo.GetMembershipAsync(kanbanId, user.Id);
            if (existing != null)
                throw new InvalidOperationException("User is already a member.");

            var member = new KanbanMember
            {
                KanbanId = kanbanId,
                UserId = user.Id,
                Role = MemberRoles.Member
            };

            await _repo.AddMemberAsync(member);
            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);

            return new MemberDto
            {
                UserId = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = MemberRoles.Member,
                JoinedAt = member.JoinedAt
            };
        }

        public async Task<bool> UpdateMemberRoleAsync(int kanbanId, int userId, int targetUserId, UpdateMemberRoleDto dto)
        {
            var creatorId = await _repo.GetKanbanCreatorIdAsync(kanbanId);
            if (creatorId == null) return false;

            if (userId != creatorId) return false;

            if (targetUserId == creatorId) return false;

            var target = await _repo.GetMembershipAsync(kanbanId, targetUserId);
            if (target == null) return false;

            target.Role = dto.Role;
            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);
            return true;
        }

        public async Task<bool> RemoveMemberAsync(int kanbanId, int userId, int targetUserId)
        {
            var membership = await _repo.GetMembershipAsync(kanbanId, userId);
            if (membership?.Role != MemberRoles.Admin) return false;

            var creatorId = await _repo.GetKanbanCreatorIdAsync(kanbanId);
            if (targetUserId == creatorId) return false;

            var target = await _repo.GetMembershipAsync(kanbanId, targetUserId);
            if (target == null) return false;

            await _repo.DeleteMemberAsync(target);
            await _repo.SaveChangesAsync();
            await Broadcast(kanbanId);
            return true;
        }

        private BoardDto MapToDto(Kanban board, int userId)
        {
            var columns = board.Columns
                .OrderBy(c => c.Order)
                .Select(c => new ColumnDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Order = c.Order,
                    TicketCount = c.Tickets.Count,
                    TotalHours = c.Tickets.Sum(t => t.TimeSpent),
                    Tickets = c.Tickets.OrderBy(t => t.Order).Select(t => MapTicketToDto(t)).ToList()
                }).ToList();

            return new BoardDto
            {
                Id = board.Id,
                Name = board.Name,
                CurrentUserRole = board.Members.FirstOrDefault(m => m.UserId == userId)?.Role ?? MemberRoles.Member,
                CreatedByUserId = board.CreatedByUserId,
                TotalHours = columns.Sum(c => c.TotalHours),
                Columns = columns,
                Members = board.Members.Select(m => new MemberDto
                {
                    UserId = m.UserId,
                    Email = m.User.Email,
                    FirstName = m.User.FirstName,
                    LastName = m.User.LastName,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt
                }).ToList()
            };
        }

        private static TicketDto MapTicketToDto(Ticket ticket)
        {
            var user = ticket.AssignedTo;
            var email = user?.Email ?? string.Empty;
            var name = user != null
                ? $"{user.FirstName} {user.LastName}".Trim()
                : string.Empty;
            return new()
            {
                Id = ticket.Id,
                Title = ticket.Title,
                Description = ticket.Description,
                TimeSpent = ticket.TimeSpent,
                ColumnId = ticket.ColumnId,
                AssignedToUserId = ticket.AssignedToUserId,
                AssignedToEmail = email,
                AssignedToName = string.IsNullOrEmpty(name) ? email : name,
                CreatedAt = ticket.CreatedAt
            };
        }
    }
}
