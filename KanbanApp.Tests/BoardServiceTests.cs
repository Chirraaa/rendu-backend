using KanbanApp.API.DTOs;
using KanbanApp.API.Hubs;
using KanbanApp.API.Models;
using KanbanApp.API.Repositories.Interfaces;
using KanbanApp.API.Services;
using Microsoft.AspNetCore.SignalR;
using Moq;

namespace KanbanApp.Tests;

public class BoardServiceTests
{
    private readonly Mock<IBoardRepository> _repoMock;
    private readonly BoardService _service;

    public BoardServiceTests()
    {
        _repoMock = new Mock<IBoardRepository>();

        var mockHub = new Mock<IHubContext<BoardHub>>();
        var mockClients = new Mock<IHubClients>();
        var mockClient = new Mock<IClientProxy>();
        mockHub.Setup(h => h.Clients).Returns(mockClients.Object);
        mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClient.Object);
        mockClient
            .Setup(c => c.SendCoreAsync(It.IsAny<string>(), It.IsAny<object?[]?>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _service = new BoardService(_repoMock.Object, mockHub.Object);
    }

    private static Kanban MakeBoard(int kanbanId, int userId, string role = MemberRoles.Admin)
    {
        var user = new User { Id = userId, Email = "u@test.com", FirstName = "U", LastName = "User" };
        var member = new KanbanMember { UserId = userId, Role = role, User = user };
        return new Kanban
        {
            Id = kanbanId,
            Name = "Test Board",
            CreatedByUserId = userId,
            Members = new List<KanbanMember> { member },
            Columns = new List<Column>()
        };
    }

    // GetBoardAsync

    [Fact]
    public async Task GetBoardAsync_ReturnsNull_WhenBoardNotFound()
    {
        _repoMock.Setup(r => r.GetBoardAsync(1)).ReturnsAsync((Kanban?)null);

        var result = await _service.GetBoardAsync(1, 1);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBoardAsync_ReturnsNull_WhenUserNotMember()
    {
        var board = MakeBoard(1, userId: 1);
        _repoMock.Setup(r => r.GetBoardAsync(1)).ReturnsAsync(board);

        var result = await _service.GetBoardAsync(1, userId: 99);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBoardAsync_ReturnsDto_WhenUserIsMember()
    {
        var board = MakeBoard(1, userId: 1);
        _repoMock.Setup(r => r.GetBoardAsync(1)).ReturnsAsync(board);

        var result = await _service.GetBoardAsync(1, userId: 1);

        Assert.NotNull(result);
        Assert.Equal("Test Board", result.Name);
        Assert.Equal(MemberRoles.Admin, result.CurrentUserRole);
    }

    // AddColumnAsync

    [Fact]
    public async Task AddColumnAsync_ReturnsNull_WhenNotAdmin()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 2))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Member });

        var result = await _service.AddColumnAsync(1, 2, new CreateColumnDto { Name = "Col" });

        Assert.Null(result);
    }

    [Fact]
    public async Task AddColumnAsync_ReturnsDto_WhenAdmin()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Admin });
        _repoMock.Setup(r => r.GetMaxColumnOrderAsync(1)).ReturnsAsync(2);
        _repoMock.Setup(r => r.AddColumnAsync(It.IsAny<Column>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.AddColumnAsync(1, 1, new CreateColumnDto { Name = "  New Col  " });

        Assert.NotNull(result);
        Assert.Equal("New Col", result.Name);
        Assert.Equal(3, result.Order);
    }

    // UpdateColumnAsync

    [Fact]
    public async Task UpdateColumnAsync_ReturnsFalse_WhenNotAdmin()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 2))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Member });

        var result = await _service.UpdateColumnAsync(1, 2, 10, new UpdateColumnDto { Name = "x" });

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateColumnAsync_ReturnsTrue_WhenAdminAndColumnExists()
    {
        var column = new Column { Id = 10, KanbanId = 1, Name = "Old" };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Admin });
        _repoMock.Setup(r => r.GetColumnInKanbanAsync(10, 1)).ReturnsAsync(column);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.UpdateColumnAsync(1, 1, 10, new UpdateColumnDto { Name = "  New Name  " });

        Assert.True(result);
        Assert.Equal("New Name", column.Name);
    }

    // DeleteColumnAsync

    [Fact]
    public async Task DeleteColumnAsync_ReturnsFalse_WhenNotAdmin()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 2))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Member });

        var result = await _service.DeleteColumnAsync(1, 2, 10);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteColumnAsync_ReturnsTrue_WhenAdminAndColumnExists()
    {
        var column = new Column { Id = 10, KanbanId = 1 };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Admin });
        _repoMock.Setup(r => r.GetColumnInKanbanAsync(10, 1)).ReturnsAsync(column);
        _repoMock.Setup(r => r.DeleteColumnAsync(column)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.DeleteColumnAsync(1, 1, 10);

        Assert.True(result);
        _repoMock.Verify(r => r.DeleteColumnAsync(column), Times.Once);
    }

    // AddTicketAsync

    [Fact]
    public async Task AddTicketAsync_ReturnsNull_WhenNotMember()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 99)).ReturnsAsync((KanbanMember?)null);

        var result = await _service.AddTicketAsync(1, 99, new CreateTicketDto { Title = "T", ColumnId = 1 });

        Assert.Null(result);
    }

    [Fact]
    public async Task AddTicketAsync_ReturnsNull_WhenColumnNotInKanban()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Member });
        _repoMock.Setup(r => r.GetColumnInKanbanAsync(5, 1)).ReturnsAsync((Column?)null);

        var result = await _service.AddTicketAsync(1, 1, new CreateTicketDto { Title = "T", ColumnId = 5 });

        Assert.Null(result);
    }

    [Fact]
    public async Task AddTicketAsync_ReturnsDto_WhenValid()
    {
        var user = new User { Id = 1, Email = "u@test.com", FirstName = "U", LastName = "User" };
        var column = new Column { Id = 5, KanbanId = 1 };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { UserId = 1, Role = MemberRoles.Member });
        _repoMock.Setup(r => r.GetColumnInKanbanAsync(5, 1)).ReturnsAsync(column);
        _repoMock.Setup(r => r.GetMaxTicketOrderAsync(5)).ReturnsAsync(0);
        _repoMock.Setup(r => r.AddTicketAsync(It.IsAny<Ticket>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.GetTicketAsync(It.IsAny<int>()))
            .ReturnsAsync(new Ticket { Id = 1, Title = "Test Ticket", ColumnId = 5, AssignedToUserId = 1, AssignedTo = user });

        var result = await _service.AddTicketAsync(1, 1, new CreateTicketDto { Title = "  Test Ticket  ", ColumnId = 5 });

        Assert.NotNull(result);
        _repoMock.Verify(r => r.AddTicketAsync(It.Is<Ticket>(t => t.Title == "Test Ticket")), Times.Once);
    }

    // UpdateTicketAsync

    [Fact]
    public async Task UpdateTicketAsync_ReturnsFalse_WhenNotMember()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 99)).ReturnsAsync((KanbanMember?)null);

        var result = await _service.UpdateTicketAsync(1, 99, 10, new UpdateTicketDto { Title = "T", ColumnId = 5 });

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateTicketAsync_ReturnsFalse_WhenMemberAndNotAssigned()
    {
        var ticket = new Ticket { Id = 10, Title = "Old", ColumnId = 5, AssignedToUserId = 3 };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 2))
            .ReturnsAsync(new KanbanMember { UserId = 2, Role = MemberRoles.Member });
        _repoMock.Setup(r => r.GetTicketInKanbanAsync(10, 1)).ReturnsAsync(ticket);

        var result = await _service.UpdateTicketAsync(1, 2, 10, new UpdateTicketDto { Title = "New", ColumnId = 5 });

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateTicketAsync_ReturnsTrue_WhenAdminUpdatesAnyTicket()
    {
        var ticket = new Ticket { Id = 10, Title = "Old", ColumnId = 5, AssignedToUserId = 3, TimeSpent = 0 };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { UserId = 1, Role = MemberRoles.Admin });
        _repoMock.Setup(r => r.GetTicketInKanbanAsync(10, 1)).ReturnsAsync(ticket);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.UpdateTicketAsync(1, 1, 10, new UpdateTicketDto { Title = "  Updated  ", ColumnId = 5, TimeSpent = 2 });

        Assert.True(result);
        Assert.Equal("Updated", ticket.Title);
        Assert.Equal(2, ticket.TimeSpent);
    }

    [Fact]
    public async Task UpdateTicketAsync_ReturnsTrue_WhenAssignedMemberUpdatesOwnTicket()
    {
        var ticket = new Ticket { Id = 10, Title = "Old", ColumnId = 5, AssignedToUserId = 2, TimeSpent = 0 };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 2))
            .ReturnsAsync(new KanbanMember { UserId = 2, Role = MemberRoles.Member });
        _repoMock.Setup(r => r.GetTicketInKanbanAsync(10, 1)).ReturnsAsync(ticket);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.UpdateTicketAsync(1, 2, 10, new UpdateTicketDto { Title = "My update", ColumnId = 5, TimeSpent = 1 });

        Assert.True(result);
        Assert.Equal("My update", ticket.Title);
        Assert.Equal(1, ticket.TimeSpent);
    }

    // DeleteTicketAsync

    [Fact]
    public async Task DeleteTicketAsync_ReturnsFalse_WhenNotMember()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 99)).ReturnsAsync((KanbanMember?)null);

        var result = await _service.DeleteTicketAsync(1, 99, 10);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteTicketAsync_ReturnsTrue_WhenAdminDeletesAnyTicket()
    {
        var ticket = new Ticket { Id = 10, ColumnId = 5, AssignedToUserId = 3 };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { UserId = 1, Role = MemberRoles.Admin });
        _repoMock.Setup(r => r.GetTicketInKanbanAsync(10, 1)).ReturnsAsync(ticket);
        _repoMock.Setup(r => r.DeleteTicketAsync(ticket)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.DeleteTicketAsync(1, 1, 10);

        Assert.True(result);
        _repoMock.Verify(r => r.DeleteTicketAsync(ticket), Times.Once);
    }

    // MoveTicketAsync

    [Fact]
    public async Task MoveTicketAsync_ReturnsFalse_WhenTargetColumnNotInKanban()
    {
        var ticket = new Ticket { Id = 10, ColumnId = 5 };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Member });
        _repoMock.Setup(r => r.GetTicketInKanbanAsync(10, 1)).ReturnsAsync(ticket);
        _repoMock.Setup(r => r.GetColumnInKanbanAsync(99, 1)).ReturnsAsync((Column?)null);

        var result = await _service.MoveTicketAsync(1, 1, 10, new MoveTicketDto { TargetColumnId = 99 });

        Assert.False(result);
    }

    [Fact]
    public async Task MoveTicketAsync_ReturnsTrue_AndRecordsHistory_WhenValid()
    {
        var ticket = new Ticket { Id = 10, ColumnId = 5, Order = 1 };
        var targetColumn = new Column { Id = 8, KanbanId = 1 };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Member });
        _repoMock.Setup(r => r.GetTicketInKanbanAsync(10, 1)).ReturnsAsync(ticket);
        _repoMock.Setup(r => r.GetColumnInKanbanAsync(8, 1)).ReturnsAsync(targetColumn);
        _repoMock.Setup(r => r.GetMaxTicketOrderAsync(8)).ReturnsAsync(2);
        _repoMock.Setup(r => r.AddMovementHistoryAsync(It.IsAny<TicketMovementHistory>())).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.MoveTicketAsync(1, 1, 10, new MoveTicketDto { TargetColumnId = 8 });

        Assert.True(result);
        Assert.Equal(8, ticket.ColumnId);
        Assert.Equal(3, ticket.Order);
        _repoMock.Verify(r => r.AddMovementHistoryAsync(It.Is<TicketMovementHistory>(h =>
            h.TicketId == 10 && h.FromColumnId == 5 && h.ToColumnId == 8)), Times.Once);
    }

    // InviteMemberAsync

    [Fact]
    public async Task InviteMemberAsync_ReturnsNull_WhenRequesterNotAdmin()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 2))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Member });

        var result = await _service.InviteMemberAsync(1, 2, new InviteMemberDto { Email = "x@test.com" });

        Assert.Null(result);
    }

    [Fact]
    public async Task InviteMemberAsync_ThrowsKeyNotFoundException_WhenEmailNotFound()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Admin });
        _repoMock.Setup(r => r.GetUserByEmailAsync("notfound@test.com")).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            _service.InviteMemberAsync(1, 1, new InviteMemberDto { Email = "notfound@test.com" }));
    }

    [Fact]
    public async Task InviteMemberAsync_ThrowsInvalidOperationException_WhenAlreadyMember()
    {
        var user = new User { Id = 5, Email = "already@test.com" };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Admin });
        _repoMock.Setup(r => r.GetUserByEmailAsync("already@test.com")).ReturnsAsync(user);
        _repoMock.Setup(r => r.GetMembershipAsync(1, 5))
            .ReturnsAsync(new KanbanMember { UserId = 5, KanbanId = 1 });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.InviteMemberAsync(1, 1, new InviteMemberDto { Email = "already@test.com" }));
    }

    // UpdateMemberRoleAsync

    [Fact]
    public async Task UpdateMemberRoleAsync_ReturnsFalse_WhenRequesterIsNotCreator()
    {
        _repoMock.Setup(r => r.GetKanbanCreatorIdAsync(1)).ReturnsAsync(1);

        var result = await _service.UpdateMemberRoleAsync(1, 2, 3, new UpdateMemberRoleDto { Role = MemberRoles.Admin });

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateMemberRoleAsync_ReturnsFalse_WhenTargetIsCreator()
    {
        _repoMock.Setup(r => r.GetKanbanCreatorIdAsync(1)).ReturnsAsync(1);

        var result = await _service.UpdateMemberRoleAsync(1, 1, 1, new UpdateMemberRoleDto { Role = MemberRoles.Member });

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateMemberRoleAsync_ReturnsTrue_WhenCreatorChangesRole()
    {
        var target = new KanbanMember { UserId = 2, KanbanId = 1, Role = MemberRoles.Member };
        _repoMock.Setup(r => r.GetKanbanCreatorIdAsync(1)).ReturnsAsync(1);
        _repoMock.Setup(r => r.GetMembershipAsync(1, 2)).ReturnsAsync(target);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.UpdateMemberRoleAsync(1, 1, 2, new UpdateMemberRoleDto { Role = MemberRoles.Admin });

        Assert.True(result);
        Assert.Equal(MemberRoles.Admin, target.Role);
    }

    // RemoveMemberAsync

    [Fact]
    public async Task RemoveMemberAsync_ReturnsFalse_WhenRequesterNotAdmin()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 2))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Member });

        var result = await _service.RemoveMemberAsync(1, 2, 3);

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveMemberAsync_ReturnsFalse_WhenTargetIsCreator()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Admin });
        _repoMock.Setup(r => r.GetKanbanCreatorIdAsync(1)).ReturnsAsync(1);

        var result = await _service.RemoveMemberAsync(1, 1, 1);

        Assert.False(result);
    }

    [Fact]
    public async Task RemoveMemberAsync_ReturnsTrue_WhenValidRemoval()
    {
        var target = new KanbanMember { UserId = 3, KanbanId = 1, Role = MemberRoles.Member };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1))
            .ReturnsAsync(new KanbanMember { Role = MemberRoles.Admin });
        _repoMock.Setup(r => r.GetKanbanCreatorIdAsync(1)).ReturnsAsync(1);
        _repoMock.Setup(r => r.GetMembershipAsync(1, 3)).ReturnsAsync(target);
        _repoMock.Setup(r => r.DeleteMemberAsync(target)).Returns(Task.CompletedTask);
        _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.RemoveMemberAsync(1, 1, 3);

        Assert.True(result);
        _repoMock.Verify(r => r.DeleteMemberAsync(target), Times.Once);
    }
}
