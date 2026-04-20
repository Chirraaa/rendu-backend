using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using KanbanApp.API.DTOs;
using KanbanApp.API.Models;
using KanbanApp.API.Repositories.Interfaces;
using KanbanApp.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace KanbanApp.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly string _jwtSecret;
        private readonly string _jwtIssuer;
        private readonly string _jwtAudience;
        private readonly int _accessTokenExpiryMinutes;
        private readonly int _refreshTokenExpiryDays;
        private readonly int _rememberMeExpiryDays;

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
            _jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")!;
            _jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")!;
            _jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")!;
            _accessTokenExpiryMinutes = int.Parse(Environment.GetEnvironmentVariable("ACCESS_TOKEN_EXPIRY_MINUTES") ?? "15");
            _refreshTokenExpiryDays = int.Parse(Environment.GetEnvironmentVariable("REFRESH_TOKEN_EXPIRY_DAYS") ?? "7");
            _rememberMeExpiryDays = int.Parse(Environment.GetEnvironmentVariable("REMEMBER_ME_EXPIRY_DAYS") ?? "30");
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, HttpResponse response)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _authRepository.GetUserByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return null;

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            var expiryDays = request.RememberMe ? _rememberMeExpiryDays : _refreshTokenExpiryDays;

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(expiryDays)
            };

            await _authRepository.AddRefreshTokenAsync(refreshTokenEntity);
            await _authRepository.SaveChangesAsync();

            SetRefreshTokenCookie(response, refreshToken, expiryDays);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
        }

        public async Task<AuthResponseDto?> RefreshAsync(HttpRequest request, HttpResponse response)
        {
            var refreshToken = request.Cookies["refreshToken"];
            if (refreshToken == null)
                return null;

            var tokenEntity = await _authRepository.GetRefreshTokenAsync(refreshToken);
            if (tokenEntity == null || tokenEntity.IsRevoked || tokenEntity.ExpiresAt < DateTime.UtcNow)
                return null;

            var user = tokenEntity.User;

            await _authRepository.RevokeRefreshTokenAsync(tokenEntity);

            var newRefreshToken = GenerateRefreshToken();
            var remainingDays = (int)(tokenEntity.ExpiresAt - DateTime.UtcNow).TotalDays;

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(remainingDays)
            };

            await _authRepository.AddRefreshTokenAsync(newRefreshTokenEntity);
            await _authRepository.SaveChangesAsync();

            SetRefreshTokenCookie(response, newRefreshToken, remainingDays);

            var accessToken = GenerateAccessToken(user);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
            };
        }

        public async Task LogoutAsync(HttpRequest request, HttpResponse response)
        {
            var refreshToken = request.Cookies["refreshToken"];
            if (refreshToken != null)
            {
                var tokenEntity = await _authRepository.GetRefreshTokenAsync(refreshToken);
                if (tokenEntity != null)
                {
                    await _authRepository.RevokeRefreshTokenAsync(tokenEntity);
                    await _authRepository.SaveChangesAsync();
                }
            }

            response.Cookies.Delete("refreshToken");
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            var existing = await _authRepository.GetUserByEmailAsync(email);
            if (existing != null)
                return null;

            var user = new User
            {
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim()
            };

            await _authRepository.CreateUserAsync(user);
            await _authRepository.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = string.Empty,
                Email = user.Email,
            };
        }

        private string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtIssuer,
                audience: _jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

        private static void SetRefreshTokenCookie(HttpResponse response, string token, int expiryDays)
        {
            response.Cookies.Append("refreshToken", token, new CookieOptions
            {
               HttpOnly = true,
               Secure = true,
               SameSite = SameSiteMode.None,
               Expires = DateTime.UtcNow.AddDays(expiryDays) 
            });
        }
    }
}