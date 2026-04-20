using KanbanApp.API.Data;
using KanbanApp.API.Models;
using KanbanApp.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KanbanApp.API.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly AppDbContext _context;

        public AuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserByEmailAsync(string email) => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            return user;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token) => await _context.RefreshTokens
        .Include(rt => rt.User)
        .FirstOrDefaultAsync(rt => rt.Token == token);

        public async Task AddRefreshTokenAsync(RefreshToken refreshToken) => await _context.RefreshTokens.AddAsync(refreshToken);

        public Task RevokeRefreshTokenAsync(RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = true;
            _context.RefreshTokens.Update(refreshToken);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    }
}