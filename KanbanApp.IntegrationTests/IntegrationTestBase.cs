using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KanbanApp.IntegrationTests;

public abstract class IntegrationTestBase
{
    protected readonly HttpClient _client;

    protected IntegrationTestBase(KanbanWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    protected async Task<string> LoginAsAdminAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "admin@kanban.com",
            password = "passwordAdmin"
        });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return body!["accessToken"].ToString()!;
    }

    protected void SetBearerToken(string token) =>
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

    protected void ClearBearerToken() =>
        _client.DefaultRequestHeaders.Authorization = null;
}
