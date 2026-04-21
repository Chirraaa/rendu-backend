using KanbanApp.API.DTOs;
using KanbanApp.API.Models;
using KanbanApp.API.Repositories.Interfaces;
using KanbanApp.API.Services;
using Microsoft.AspNetCore.Http;
using Moq;

namespace KanbanApp.Tests;

public class AuthServiceTests
{
    private readonly Mock<IAuthRepository> _repoMock;
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        Environment.SetEnvironmentVariable("JWT_SECRET", "super-secret-key-for-testing-1234567890");
        Environment.SetEnvironmentVariable("JWT_ISSUER", "test-issuer");
        Environment.SetEnvironmentVariable("JWT_AUDIENCE", "test-audience");
        Environment.SetEnvironmentVariable("ACCESS_TOKEN_EXPIRY_MINUTES", "15");
        Environment.SetEnvironmentVariable("REFRESH_TOKEN_EXPIRY_DAYS", "7");
        Environment.SetEnvironmentVariable("REMEMBER_ME_EXPIRY_DAYS", "30");

        _repoMock = new Mock<IAuthRepository>();
        _service = new AuthService(_repoMock.Object);
    }

    private static DefaultHttpContext MakeHttpContext(string? refreshTokenCookie = null)
    {
        var ctx = new DefaultHttpContext();
        if (refreshTokenCookie != null)
            ctx.Request.Headers["Cookie"] = $"refreshToken={refreshTokenCookie}";
        return ctx;
    }

    // RegisterAsync

    [Fact]
    public async Task RegisterAsync_ReturnsNull_WhenEmailAlreadyExists()
    {
        _repoMock.Setup(r => r.GetUserByEmailAsync("taken@test.com"))
            .ReturnsAsync(new User { Email = "taken@test.com" });

        var result = await _service.RegisterAsync(new RegisterRequestDto
        {
            Email = "taken@test.com",
            Password = "Password1!",
            FirstName = "Jane",
            LastName = "Doe"
        });

        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsDto_WhenEmailIsNew()
    {
        _repoMock.Setup(r => r.GetUserByEmailAsync("new@test.com")).ReturnsAsync((User?)null);
        _repoMock.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(new User());

        var result = await _service.RegisterAsync(new RegisterRequestDto
        {
            Email = "new@test.com",
            Password = "Password1!",
            FirstName = "Jane",
            LastName = "Doe"
        });

        Assert.NotNull(result);
        Assert.Equal("new@test.com", result.Email);
        _repoMock.Verify(r => r.CreateUserAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_TrimsAndLowercasesEmail()
    {
        _repoMock.Setup(r => r.GetUserByEmailAsync("trimmed@test.com")).ReturnsAsync((User?)null);
        _repoMock.Setup(r => r.CreateUserAsync(It.IsAny<User>())).ReturnsAsync(new User());

        var result = await _service.RegisterAsync(new RegisterRequestDto
        {
            Email = "  TRIMMED@test.com  ",
            Password = "Password1!",
            FirstName = "Jane",
            LastName = "Doe"
        });

        Assert.NotNull(result);
        Assert.Equal("trimmed@test.com", result.Email);
    }

    // LoginAsync

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenUserNotFound()
    {
        _repoMock.Setup(r => r.GetUserByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
        var ctx = MakeHttpContext();

        var result = await _service.LoginAsync(
            new LoginRequestDto { Email = "nobody@test.com", Password = "pass" },
            ctx.Response);

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsNull_WhenPasswordWrong()
    {
        var user = new User { Email = "user@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct") };
        _repoMock.Setup(r => r.GetUserByEmailAsync("user@test.com")).ReturnsAsync(user);
        var ctx = MakeHttpContext();

        var result = await _service.LoginAsync(
            new LoginRequestDto { Email = "user@test.com", Password = "wrong" },
            ctx.Response);

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_ReturnsDto_OnValidCredentials()
    {
        var user = new User { Id = 1, Email = "user@test.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("correct"), FirstName = "John", LastName = "Doe" };
        _repoMock.Setup(r => r.GetUserByEmailAsync("user@test.com")).ReturnsAsync(user);
        _repoMock.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var ctx = MakeHttpContext();

        var result = await _service.LoginAsync(
            new LoginRequestDto { Email = "user@test.com", Password = "correct" },
            ctx.Response);

        Assert.NotNull(result);
        Assert.Equal("user@test.com", result.Email);
        Assert.NotEmpty(result.AccessToken);
        _repoMock.Verify(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    // LogoutAsync

    [Fact]
    public async Task LogoutAsync_RevokesToken_WhenCookiePresent()
    {
        var token = new RefreshToken { Token = "abc", IsRevoked = false };
        _repoMock.Setup(r => r.GetRefreshTokenAsync("abc")).ReturnsAsync(token);
        _repoMock.Setup(r => r.RevokeRefreshTokenAsync(token)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var ctx = MakeHttpContext("abc");

        await _service.LogoutAsync(ctx.Request, ctx.Response);

        _repoMock.Verify(r => r.RevokeRefreshTokenAsync(token), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task LogoutAsync_DoesNothing_WhenNoCookie()
    {
        var ctx = MakeHttpContext();

        await _service.LogoutAsync(ctx.Request, ctx.Response);

        _repoMock.Verify(r => r.GetRefreshTokenAsync(It.IsAny<string>()), Times.Never);
    }

    // RefreshAsync

    [Fact]
    public async Task RefreshAsync_ReturnsNull_WhenNoCookie()
    {
        var ctx = MakeHttpContext();
        var result = await _service.RefreshAsync(ctx.Request, ctx.Response);
        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_ReturnsNull_WhenTokenRevoked()
    {
        _repoMock.Setup(r => r.GetRefreshTokenAsync("abc"))
            .ReturnsAsync(new RefreshToken { IsRevoked = true, ExpiresAt = DateTime.UtcNow.AddDays(1) });
        var ctx = MakeHttpContext("abc");

        var result = await _service.RefreshAsync(ctx.Request, ctx.Response);

        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_ReturnsNull_WhenTokenExpired()
    {
        _repoMock.Setup(r => r.GetRefreshTokenAsync("abc"))
            .ReturnsAsync(new RefreshToken { IsRevoked = false, ExpiresAt = DateTime.UtcNow.AddDays(-1) });
        var ctx = MakeHttpContext("abc");

        var result = await _service.RefreshAsync(ctx.Request, ctx.Response);

        Assert.Null(result);
    }

    [Fact]
    public async Task RefreshAsync_ReturnsNewToken_WhenValid()
    {
        var user = new User { Id = 1, Email = "user@test.com", FirstName = "John", LastName = "Doe" };
        var token = new RefreshToken { IsRevoked = false, ExpiresAt = DateTime.UtcNow.AddDays(5), User = user, UserId = 1 };
        _repoMock.Setup(r => r.GetRefreshTokenAsync("abc")).ReturnsAsync(token);
        _repoMock.Setup(r => r.RevokeRefreshTokenAsync(token)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        var ctx = MakeHttpContext("abc");

        var result = await _service.RefreshAsync(ctx.Request, ctx.Response);

        Assert.NotNull(result);
        Assert.Equal("user@test.com", result.Email);
        Assert.NotEmpty(result.AccessToken);
        _repoMock.Verify(r => r.RevokeRefreshTokenAsync(token), Times.Once);
        _repoMock.Verify(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Once);
    }
}
