using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace KanbanApp.API.Hubs
{
    [Authorize]
    public class BoardHub : Hub
    {
        public async Task JoinBoard(int kanbanId) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, $"board-{kanbanId}");

        public async Task LeaveBoard(int kanbanId) =>
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"board-{kanbanId}");
    }
}
