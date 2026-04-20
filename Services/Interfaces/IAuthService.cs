using KanbanApp.API.DTOs;

namespace KanbanApp.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, HttpResponse response);
        Task<AuthResponseDto?> RefreshAsync(HttpRequest request, HttpResponse response);
        Task LogoutAsync(HttpRequest request, HttpResponse response);
        Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request); 
    }
}