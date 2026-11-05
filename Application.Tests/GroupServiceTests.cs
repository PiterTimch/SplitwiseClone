using Microsoft.AspNetCore.Identity;
using Moq;
using SplitwiseClone.Application.DTOs.Auth;
using SplitwiseClone.Application.DTOs.Expenses;
using SplitwiseClone.Application.DTOs.Groups;
using SplitwiseClone.Application.DTOs.Users;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Application.Services;
using SplitwiseClone.Domain.Entities;
using Xunit;

namespace SplitwiseClone.Application.Tests;

public class GroupServiceTests
{
    private readonly Mock<IUnitOfWork> _work = new();
    private readonly Mock<IGroupRepository> _groups = new();
    private readonly Mock<IGroupMemberRepository> _members = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly GroupService _service;

    public GroupServiceTests()
    {
        _work.SetupGet(x => x.Groups).Returns(_groups.Object);
        _work.SetupGet(x => x.GroupMembers).Returns(_members.Object);
        _work.SetupGet(x => x.Users).Returns(_users.Object);
        _service = new GroupService(_work.Object);
    }

    [Fact]
    public async Task GetAllAsync_MapsGroupsAndMembers()
    {
        var group = new Group { Id = 4, Name = "Trip" };
        var user = new User { Name = "Alex", Email = "a@b.com" };
        _groups.Setup(x => x.GetAllAsync(default)).ReturnsAsync(new[] { group });
        _members.Setup(x => x.GetByGroupIdAsync(4, default)).ReturnsAsync(new[] { new GroupMember { UserId = user.Id, User = user } });

        var result = (await _service.GetAllAsync()).Single();

        Assert.Equal("Trip", result.Name);
        Assert.Equal("Alex", result.Members.Single().UserName);
        Assert.Equal("a@b.com", result.Members.Single().UserEmail);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetByIdAsync_ReturnsGroupOrNull(bool exists)
    {
        var group = exists ? new Group { Id = 2, Name = "Home", Members = new List<GroupMember> { new() { User = new User { Name = "Sam", Email = "s@b.com" } } } } : null;
        _groups.Setup(x => x.GetByIdWithDetailsAsync(2, default)).ReturnsAsync(group);

        var result = await _service.GetByIdAsync(2);

        if (exists)
        {
            Assert.NotNull(result);
            Assert.Equal("Home", result.Name);
            Assert.Equal("Sam", result.Members.Single().UserName);
        }
        else Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_AddsGroupAndReturnsEmptyMembers()
    {
        _groups.Setup(x => x.AddAsync(It.IsAny<Group>(), default)).Callback<Group, CancellationToken>((g, _) => g.Id = 8).Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(new CreateGroupDto("Trip"));

        Assert.Equal(8, result.Id);
        Assert.Equal("Trip", result.Name);
        Assert.Empty(result.Members);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesGroup()
    {
        var group = new Group { Id = 2, Name = "Old" };
        _groups.Setup(x => x.GetByIdAsync(2, default)).ReturnsAsync(group);

        await _service.UpdateAsync(2, new UpdateGroupDto("New"));

        Assert.Equal("New", group.Name);
        _groups.Verify(x => x.Update(group), Times.Once);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenGroupMissing()
    {
        _groups.Setup(x => x.GetByIdAsync(2, default)).ReturnsAsync((Group?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateAsync(2, new("New")));

        _work.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DeletesGroup()
    {
        var group = new Group { Id = 2 };
        _groups.Setup(x => x.GetByIdAsync(2, default)).ReturnsAsync(group);

        await _service.DeleteAsync(2);

        _groups.Verify(x => x.Delete(group), Times.Once);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenGroupMissing()
    {
        _groups.Setup(x => x.GetByIdAsync(2, default)).ReturnsAsync((Group?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteAsync(2));

        _work.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Theory]
    [InlineData(false, true, false, "EntityNotFoundException")]
    [InlineData(true, false, false, "EntityNotFoundException")]
    [InlineData(true, true, true, "EntityConflictException")]
    public async Task AddMemberAsync_RejectsInvalidMembership(bool groupExists, bool userExists, bool alreadyMember, string exception)
    {
        var userId = Guid.NewGuid();
        _groups.Setup(x => x.GetByIdAsync(3, default)).ReturnsAsync(groupExists ? new Group { Id = 3 } : null);
        _users.Setup(x => x.GetByIdAsync(userId, default)).ReturnsAsync(userExists ? new User { Id = userId } : null);
        _members.Setup(x => x.GetAsync(3, userId, default)).ReturnsAsync(alreadyMember ? new GroupMember() : null);

        if (exception == "EntityNotFoundException")
            await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.AddMemberAsync(3, new(userId)));
        else
            await Assert.ThrowsAsync<EntityConflictException>(() => _service.AddMemberAsync(3, new(userId)));

        _work.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task AddMemberAsync_AddsMemberAndReturnsUserDetails()
    {
        var userId = Guid.NewGuid();
        _groups.Setup(x => x.GetByIdAsync(3, default)).ReturnsAsync(new Group { Id = 3 });
        _users.Setup(x => x.GetByIdAsync(userId, default)).ReturnsAsync(new User { Id = userId, Name = "Sam", Email = "s@b.com" });
        _members.Setup(x => x.GetAsync(3, userId, default)).ReturnsAsync((GroupMember?)null);

        var result = await _service.AddMemberAsync(3, new(userId));

        Assert.Equal(userId, result.UserId);
        Assert.Equal("Sam", result.UserName);
        Assert.Equal("s@b.com", result.UserEmail);
        _members.Verify(x => x.AddAsync(It.Is<GroupMember>(m => m.GroupId == 3 && m.UserId == userId), default), Times.Once);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RemoveMemberAsync_RemovesMember()
    {
        var member = new GroupMember { GroupId = 3, UserId = Guid.NewGuid() };
        _members.Setup(x => x.GetAsync(3, member.UserId, default)).ReturnsAsync(member);

        await _service.RemoveMemberAsync(3, member.UserId);

        _members.Verify(x => x.Delete(member), Times.Once);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task RemoveMemberAsync_ThrowsWhenMemberMissing()
    {
        var id = Guid.NewGuid();
        _members.Setup(x => x.GetAsync(3, id, default)).ReturnsAsync((GroupMember?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.RemoveMemberAsync(3, id));

        _work.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }
}

