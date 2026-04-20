using KanbanApp.API.Models;

namespace KanbanApp.API.Repositories.Interfaces
{
    public interface IAuthRepository
    {
        Task<User?> GetUserByEmailAsync(string email);
        Task<User> CreateUserAsync(User user);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task AddRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeRefreshTokenAsync(RefreshToken refreshToken);
        Task SaveChangesAsync();
    }
}