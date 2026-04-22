using System.Net;
using System.Net.Http.Json;

namespace KanbanApp.IntegrationTests;

public class AuthIntegrationTests : IntegrationTestBase, IClassFixture<KanbanWebAppFactory>
{
    public AuthIntegrationTests(KanbanWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_ReturnsOk_WithValidData()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new
        {
            firstName = "John",
            lastName = "Doe",
            email = "john.doe@test.com",
            password = "Password1!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var payload = new
        {
            firstName = "Adam",
            lastName = "Admin",
            email = "admin@kanban.com", // seeded by KanbanWebAppFactory.CreateHost
            password = "Password1!"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", payload);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsOk_WithValidCredentials()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@kanban.com",
            password = "passwordAdmin"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.True(body.ContainsKey("accessToken"));
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WithWrongPassword()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@kanban.com",
            password = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WithUnknownEmail()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "nobody@test.com",
            password = "Password1!"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Refresh_ReturnsUnauthorized_WithoutToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", new { refreshToken = "" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Logout_ReturnsOk()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/logout", new { refreshToken = "" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
