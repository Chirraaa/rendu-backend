using KanbanApp.API.DTOs;
using KanbanApp.API.Models;
using KanbanApp.API.Repositories.Interfaces;
using KanbanApp.API.Services;
using Moq;

namespace KanbanApp.Tests;

public class KanbanServiceTests
{
    private readonly Mock<IKanbanRepository> _repoMock;
    private readonly KanbanService _service;

    public KanbanServiceTests()
    {
        _repoMock = new Mock<IKanbanRepository>();
        _service = new KanbanService(_repoMock.Object);
    }

    // GetUserKanbansAsync

    [Fact]
    public async Task GetUserKanbansAsync_ReturnsMappedDtos()
    {
        var kanbans = new List<Kanban>
        {
            new Kanban
            {
                Id = 1, Name = "Board A", CreatedAt = DateTime.UtcNow,
                Members = new List<KanbanMember> { new() { UserId = 10, Role = MemberRoles.Admin } }
            }
        };
        _repoMock.Setup(r => r.GetUserKanbansAsync(10)).ReturnsAsync(kanbans);

        var result = await _service.GetUserKanbansAsync(10);

        Assert.Single(result);
        Assert.Equal("Board A", result[0].Name);
        Assert.Equal(MemberRoles.Admin, result[0].Role);
        Assert.Equal(1, result[0].MemberCount);
    }

    [Fact]
    public async Task GetUserKanbansAsync_ReturnsEmptyList_WhenNoKanbans()
    {
        _repoMock.Setup(r => r.GetUserKanbansAsync(99)).ReturnsAsync(new List<Kanban>());

        var result = await _service.GetUserKanbansAsync(99);

        Assert.Empty(result);
    }

    // CreateKanbanAsync

    [Fact]
    public async Task CreateKanbanAsync_ReturnsDto_WithAdminRole()
    {
        _repoMock.Setup(r => r.CreateKanbanAsync(It.IsAny<Kanban>(), It.IsAny<KanbanMember>()))
            .ReturnsAsync(new Kanban());

        var result = await _service.CreateKanbanAsync(5, new CreateKanbanDto { Name = "  My Board  " });

        Assert.Equal("My Board", result.Name);
        Assert.Equal(MemberRoles.Admin, result.Role);
        Assert.Equal(1, result.MemberCount);
        _repoMock.Verify(r => r.CreateKanbanAsync(
            It.Is<Kanban>(k => k.Name == "My Board" && k.CreatedByUserId == 5),
            It.Is<KanbanMember>(m => m.UserId == 5 && m.Role == MemberRoles.Admin)),
            Times.Once);
    }

    // DeleteOrLeaveKanbanAsync

    [Fact]
    public async Task DeleteOrLeaveKanbanAsync_ReturnsFalse_WhenNotMember()
    {
        _repoMock.Setup(r => r.GetMembershipAsync(1, 99)).ReturnsAsync((KanbanMember?)null);

        var result = await _service.DeleteOrLeaveKanbanAsync(1, 99);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteOrLeaveKanbanAsync_DeletesKanban_WhenAdmin()
    {
        var membership = new KanbanMember { UserId = 1, KanbanId = 1, Role = MemberRoles.Admin };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 1)).ReturnsAsync(membership);
        _repoMock.Setup(r => r.DeleteKanbanAsync(1)).Returns(Task.CompletedTask);

        var result = await _service.DeleteOrLeaveKanbanAsync(1, 1);

        Assert.True(result);
        _repoMock.Verify(r => r.DeleteKanbanAsync(1), Times.Once);
        _repoMock.Verify(r => r.RemoveMemberAndUnassignTicketsAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task DeleteOrLeaveKanbanAsync_LeavesMember_WhenNotAdmin()
    {
        var membership = new KanbanMember { UserId = 2, KanbanId = 1, Role = MemberRoles.Member };
        _repoMock.Setup(r => r.GetMembershipAsync(1, 2)).ReturnsAsync(membership);
        _repoMock.Setup(r => r.RemoveMemberAndUnassignTicketsAsync(1, 2)).Returns(Task.CompletedTask);

        var result = await _service.DeleteOrLeaveKanbanAsync(1, 2);

        Assert.True(result);
        _repoMock.Verify(r => r.RemoveMemberAndUnassignTicketsAsync(1, 2), Times.Once);
        _repoMock.Verify(r => r.DeleteKanbanAsync(It.IsAny<int>()), Times.Never);
    }
}
