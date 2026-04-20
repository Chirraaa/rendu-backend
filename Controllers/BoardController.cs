using KanbanApp.API.DTOs;
using KanbanApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanApp.API.Controllers
{
    [ApiController]
    [Route("api/kanbans/{kanbanId}")]
    [Authorize]
    public class BoardController : ApiControllerBase
    {
        private readonly IBoardService _boardService;

        public BoardController(IBoardService boardService)
        {
            _boardService = boardService;
        }

        [HttpGet]
        public async Task<IActionResult> GetBoard(int kanbanId)
        {
            var board = await _boardService.GetBoardAsync(kanbanId, GetUserId());
            if (board == null) return NotFound();
            return Ok(board);
        }

        [HttpPost("columns")]
        public async Task<IActionResult> AddColumn(int kanbanId, [FromBody] CreateColumnDto dto)
        {
            var result = await _boardService.AddColumnAsync(kanbanId, GetUserId(), dto);
            if (result == null) return Forbid();
            return StatusCode(201, result);
        }

        [HttpPut("columns/{columnId}")]
        public async Task<IActionResult> UpdateColumn(int kanbanId, int columnId, [FromBody] UpdateColumnDto dto)
        {
            var result = await _boardService.UpdateColumnAsync(kanbanId, GetUserId(), columnId, dto);
            if (!result) return Forbid();
            return Ok();
        }

        [HttpDelete("columns/{columnId}")]
        public async Task<IActionResult> DeleteColumn(int kanbanId, int columnId)
        {
            var result = await _boardService.DeleteColumnAsync(kanbanId, GetUserId(), columnId);
            if (!result) return Forbid();
            return Ok();
        }

        [HttpPut("columns/reorder")]
        public async Task<IActionResult> ReorderColumns(int kanbanId, [FromBody] ReorderColumnsDto dto)
        {
            var result = await _boardService.ReorderColumnsAsync(kanbanId, GetUserId(), dto);
            if (!result) return Forbid();
            return Ok();
        }

        [HttpPost("tickets")]
        public async Task<IActionResult> AddTicket(int kanbanId, [FromBody] CreateTicketDto dto)
        {
            var result = await _boardService.AddTicketAsync(kanbanId, GetUserId(), dto);
            if (result == null) return Forbid();
            return StatusCode(201, result);
        }

        [HttpPut("tickets/{ticketId}")]
        public async Task<IActionResult> UpdateTicket(int kanbanId, int ticketId, [FromBody] UpdateTicketDto dto)
        {
            var result = await _boardService.UpdateTicketAsync(kanbanId, GetUserId(), ticketId, dto);
            if (!result) return Forbid();
            return Ok();
        }

        [HttpDelete("tickets/{ticketId}")]
        public async Task<IActionResult> DeleteTicket(int kanbanId, int ticketId)
        {
            var result = await _boardService.DeleteTicketAsync(kanbanId, GetUserId(), ticketId);
            if (!result) return Forbid();
            return Ok();
        }

        [HttpPut("tickets/{ticketId}/move")]
        public async Task<IActionResult> MoveTicket(int kanbanId, int ticketId, [FromBody] MoveTicketDto dto)
        {
            var result = await _boardService.MoveTicketAsync(kanbanId, GetUserId(), ticketId, dto);
            if (!result) return Forbid();
            return Ok();
        }

        [HttpPut("columns/{columnId}/tickets/reorder")]
        public async Task<IActionResult> ReorderTickets(int kanbanId, int columnId, [FromBody] ReorderTicketsDto dto)
        {
            var result = await _boardService.ReorderTicketsAsync(kanbanId, GetUserId(), columnId, dto);
            if (!result) return Forbid();
            return Ok();
        }

        [HttpGet("tickets/{ticketId}/history")]
        public async Task<IActionResult> GetTicketHistory(int kanbanId, int ticketId)
        {
            var result = await _boardService.GetTicketHistoryAsync(kanbanId, GetUserId(), ticketId);
            if (result == null) return Forbid();
            return Ok(result);
        }

        [HttpPost("members")]
        public async Task<IActionResult> InviteMember(int kanbanId, [FromBody] InviteMemberDto dto)
        {
            var result = await _boardService.InviteMemberAsync(kanbanId, GetUserId(), dto);
            if (result == null) return Forbid();
            return StatusCode(201, result);
        }

        [HttpPut("members/{targetUserId}")]
        public async Task<IActionResult> UpdateMemberRole(int kanbanId, int targetUserId, [FromBody] UpdateMemberRoleDto dto)
        {
            var result = await _boardService.UpdateMemberRoleAsync(kanbanId, GetUserId(), targetUserId, dto);
            if (!result) return Forbid();
            return Ok();
        }

        [HttpDelete("members/{targetUserId}")]
        public async Task<IActionResult> RemoveMember(int kanbanId, int targetUserId)
        {
            var result = await _boardService.RemoveMemberAsync(kanbanId, GetUserId(), targetUserId);
            if (!result) return Forbid();
            return Ok();
        }
    }
}
