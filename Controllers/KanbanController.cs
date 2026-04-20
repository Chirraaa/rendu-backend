using KanbanApp.API.DTOs;
using KanbanApp.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KanbanApp.API.Controllers
{
    [ApiController]
    [Route("api/kanbans")]
    [Authorize]
    public class KanbanController : ApiControllerBase
    {
        private readonly IKanbanService _kanbanService;

        public KanbanController(IKanbanService kanbanService)
        {
            _kanbanService = kanbanService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyKanbans()
        {
            var kanbans = await _kanbanService.GetUserKanbansAsync(GetUserId());
            return Ok(kanbans);
        }

        [HttpPost]
        public async Task<IActionResult> CreateKanban([FromBody] CreateKanbanDto dto)
        {
            var kanban = await _kanbanService.CreateKanbanAsync(GetUserId(), dto);
            return StatusCode(201, kanban);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrLeaveKanban(int id)
        {
            var success = await _kanbanService.DeleteOrLeaveKanbanAsync(id, GetUserId());
            if (!success) return NotFound();
            return NoContent();
        }
    }
}
