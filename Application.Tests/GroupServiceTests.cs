using Moq;
using SplitwiseClone.Application.DTOs.Groups;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Application.Services;
using SplitwiseClone.Domain.Entities;
using Xunit;

namespace SplitwiseClone.Application.Tests;

public sealed class GroupServiceTests
{
    [Fact]
    public async Task CreateAsync_AddsCreatorAsMember()
    {
        var fixture = new Fixture();
        fixture.Users.Setup(repository => repository.GetByIdAsync(7, default)).ReturnsAsync(new ApplicationUser { Id = 7 });
        fixture.Groups.Setup(repository => repository.AddAsync(It.IsAny<Group>(), default))
            .Callback<Group, CancellationToken>((group, _) => group.Id = 4).Returns(Task.CompletedTask);
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(4, default))
            .ReturnsAsync(new Group { Id = 4, Name = "Trip", Members = [new GroupMember { UserId = 7 }] });
        var result = await fixture.Service.CreateAsync(7, new CreateGroupDto("Trip"));
        Assert.Equal(7, Assert.Single(result.Members).UserId);
    }

    [Fact]
    public async Task GetByIdAsync_RejectsNonMember()
    {
        var fixture = new Fixture();
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(4, default))
            .ReturnsAsync(new Group { Id = 4, Members = [new GroupMember { UserId = 1 }] });
        await Assert.ThrowsAsync<GroupAccessDeniedException>(() => fixture.Service.GetByIdAsync(4, 2));
    }

    [Fact]
    public async Task GetByIdAsync_ThrowsTypedNotFound()
    {
        var fixture = new Fixture();
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(4, default)).ReturnsAsync((Group?)null);
        await Assert.ThrowsAsync<EntityNotFoundException>(() => fixture.Service.GetByIdAsync(4, 2));
    }

    [Fact]
    public async Task AddMemberAsync_RejectsDuplicateMember()
    {
        var fixture = new Fixture();
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(4, default))
            .ReturnsAsync(new Group { Id = 4, Members = [new GroupMember { UserId = 1 }] });
        fixture.Users.Setup(repository => repository.GetByEmailAsync("member@example.com", default)).ReturnsAsync(new ApplicationUser { Id = 3 });
        fixture.Members.Setup(repository => repository.GetAsync(4, 3, default)).ReturnsAsync(new GroupMember());
        await Assert.ThrowsAsync<EntityConflictException>(() => fixture.Service.AddMemberAsync(4, 1, new AddGroupMemberDto("member@example.com")));
        fixture.Unit.Verify(unit => unit.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task AddMemberAsync_FindsUserByEmail()
    {
        var fixture = new Fixture();
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(4, default))
            .ReturnsAsync(new Group { Id = 4, Members = [new GroupMember { UserId = 1 }] });
        fixture.Users.Setup(repository => repository.GetByEmailAsync("member@example.com", default))
            .ReturnsAsync(new ApplicationUser { Id = 3, Email = "member@example.com" });
        fixture.Members.Setup(repository => repository.AddAsync(It.IsAny<GroupMember>(), default))
            .Callback<GroupMember, CancellationToken>((member, _) => member.User = new ApplicationUser { Id = 3, Email = "member@example.com" })
            .Returns(Task.CompletedTask);

        var result = await fixture.Service.AddMemberAsync(4, 1, new AddGroupMemberDto("member@example.com"));

        Assert.Equal(3, result.UserId);
        fixture.Users.Verify(repository => repository.GetByEmailAsync("member@example.com", default), Times.Once);
    }

    [Fact]
    public async Task AddMemberAsync_ThrowsNotFoundForUnknownEmail()
    {
        var fixture = new Fixture();
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(4, default))
            .ReturnsAsync(new Group { Id = 4, Members = [new GroupMember { UserId = 1 }] });
        fixture.Users.Setup(repository => repository.GetByEmailAsync("missing@example.com", default))
            .ReturnsAsync((ApplicationUser?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => fixture.Service.AddMemberAsync(4, 1, new AddGroupMemberDto("missing@example.com")));
        fixture.Unit.Verify(unit => unit.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_RejectsGroupsWithExpenseHistory()
    {
        var fixture = new Fixture();
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(4, default))
            .ReturnsAsync(new Group { Id = 4, Members = [new GroupMember { UserId = 1 }] });
        fixture.Expenses.Setup(repository => repository.GetByGroupIdAsync(4, default))
            .ReturnsAsync([new Expense { Id = 9, GroupId = 4 }]);

        await Assert.ThrowsAsync<EntityConflictException>(() => fixture.Service.DeleteAsync(4, 1));
        fixture.Unit.Verify(unit => unit.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task RemoveMemberAsync_RejectsMembersWithExpenseHistory()
    {
        var fixture = new Fixture();
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(4, default))
            .ReturnsAsync(new Group { Id = 4, Members = [new GroupMember { UserId = 1 }, new GroupMember { UserId = 2 }] });
        fixture.Members.Setup(repository => repository.GetAsync(4, 2, default))
            .ReturnsAsync(new GroupMember { GroupId = 4, UserId = 2 });
        fixture.Expenses.Setup(repository => repository.GetByGroupIdAsync(4, default))
            .ReturnsAsync([new Expense { Id = 9, GroupId = 4, Splits = [new ExpenseSplit { UserId = 2 }] }]);

        await Assert.ThrowsAsync<EntityConflictException>(() => fixture.Service.RemoveMemberAsync(4, 2, 1));
        fixture.Unit.Verify(unit => unit.SaveChangesAsync(default), Times.Never);
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public Mock<IGroupRepository> Groups { get; } = new();
        public Mock<IGroupMemberRepository> Members { get; } = new();
        public Mock<IUserRepository> Users { get; } = new();
        public Mock<IExpenseRepository> Expenses { get; } = new();
        public GroupService Service { get; }

        public Fixture()
        {
            Unit.SetupGet(unit => unit.Groups).Returns(Groups.Object);
            Unit.SetupGet(unit => unit.GroupMembers).Returns(Members.Object);
            Unit.SetupGet(unit => unit.Users).Returns(Users.Object);
            Unit.SetupGet(unit => unit.Expenses).Returns(Expenses.Object);
            Service = new GroupService(Unit.Object);
        }
    }
}
