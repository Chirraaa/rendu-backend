using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace KanbanApp.API.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected int GetUserId()
        {
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(raw, out var id))
                throw new UnauthorizedAccessException("Invalid user identity.");
            return id;
        }
    }
}
