using System.Net;
using System.Net.Http.Json;

namespace KanbanApp.IntegrationTests;

public class BoardIntegrationTests : IntegrationTestBase, IClassFixture<KanbanWebAppFactory>
{
    public BoardIntegrationTests(KanbanWebAppFactory factory) : base(factory) { }

    private async Task<int> CreateKanbanAsync(string token, string name = "Test Board")
    {
        SetBearerToken(token);
        var response = await _client.PostAsJsonAsync("/api/kanbans", new { name });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return int.Parse(body!["id"].ToString()!);
    }

    private async Task<int> CreateColumnAsync(string token, int kanbanId, string name = "To Do")
    {
        SetBearerToken(token);
        var response = await _client.PostAsJsonAsync($"/api/kanbans/{kanbanId}/columns", new { name });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        return int.Parse(body!["id"].ToString()!);
    }

    [Fact]
    public async Task GetBoard_ReturnsOk_WhenMember()
    {
        var token = await LoginAsAdminAsync();
        var kanbanId = await CreateKanbanAsync(token, "Board Get Test");
        SetBearerToken(token);

        var response = await _client.GetAsync($"/api/kanbans/{kanbanId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.Equal("Board Get Test", body["name"].ToString());
    }

    [Fact]
    public async Task GetBoard_ReturnsNotFound_ForNonExistentKanban()
    {
        var token = await LoginAsAdminAsync();
        SetBearerToken(token);

        var response = await _client.GetAsync("/api/kanbans/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task AddColumn_ReturnsCreated_WhenAdmin()
    {
        var token = await LoginAsAdminAsync();
        var kanbanId = await CreateKanbanAsync(token, "Column Test Board");
        SetBearerToken(token);

        var response = await _client.PostAsJsonAsync($"/api/kanbans/{kanbanId}/columns", new { name = "In Progress" });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.Equal("In Progress", body["name"].ToString());
    }

    [Fact]
    public async Task AddTicket_ReturnsCreated_WhenMember()
    {
        var token = await LoginAsAdminAsync();
        var kanbanId = await CreateKanbanAsync(token, "Ticket Test Board");
        var columnId = await CreateColumnAsync(token, kanbanId, "Backlog");
        SetBearerToken(token);

        var response = await _client.PostAsJsonAsync($"/api/kanbans/{kanbanId}/tickets", new
        {
            title = "My first ticket",
            columnId,
            timeSpent = 0
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        Assert.NotNull(body);
        Assert.Equal("My first ticket", body["title"].ToString());
    }

    [Fact]
    public async Task MoveTicket_ReturnsOk_WhenValid()
    {
        var token = await LoginAsAdminAsync();
        var kanbanId = await CreateKanbanAsync(token, "Move Ticket Board");
        var col1 = await CreateColumnAsync(token, kanbanId, "Todo");
        var col2 = await CreateColumnAsync(token, kanbanId, "Done");
        SetBearerToken(token);

        var ticketResponse = await _client.PostAsJsonAsync($"/api/kanbans/{kanbanId}/tickets", new
        {
            title = "Move me",
            columnId = col1,
            timeSpent = 0
        });
        ticketResponse.EnsureSuccessStatusCode();
        var ticketBody = await ticketResponse.Content.ReadFromJsonAsync<Dictionary<string, object>>();
        var ticketId = ticketBody!["id"].ToString();

        var moveResponse = await _client.PutAsJsonAsync(
            $"/api/kanbans/{kanbanId}/tickets/{ticketId}/move",
            new { targetColumnId = col2 });

        Assert.Equal(HttpStatusCode.OK, moveResponse.StatusCode);
    }
}
