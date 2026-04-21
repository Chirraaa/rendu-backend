using System.Net;
using System.Net.Http.Json;

namespace KanbanApp.IntegrationTests;

public class KanbanIntegrationTests : IntegrationTestBase, IClassFixture<KanbanWebAppFactory>
{
    public KanbanIntegrationTests(KanbanWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task GetKanbans_ReturnsUnauthorized_WithoutToken()
    {
        ClearBearerToken();

        var response = await _client.GetAsync("/api/kanbans");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetKanbans_ReturnsOk_WithValidToken()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var response = await _client.GetAsync("/api/kanbans");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateKanban_ReturnsCreated_WithValidData()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var response = await _client.PostAsJsonAsync("/api/kanbans", new { name = "My Test Board" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.Equal("My Test Board", body["name"].ToString());
    }

    [Fact]
    public async Task DeleteKanban_ReturnsNotFound_ForNonExistentKanban()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var response = await _client.DeleteAsync("/api/kanbans/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndDeleteKanban_WorksEndToEnd()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var createResponse = await _client.PostAsJsonAsync("/api/kanbans", new { name = "Temp Board" });
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var id = created!["id"].ToString();

        var deleteResponse = await _client.DeleteAsync($"/api/kanbans/{id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);
    }
}
