using KanbanApp.API.DTOs;

namespace KanbanApp.API.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto?> RefreshAsync(string refreshToken);
        Task LogoutAsync(string refreshToken);
        Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request); 
    }
}