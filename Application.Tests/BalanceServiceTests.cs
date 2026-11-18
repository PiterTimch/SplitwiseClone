using Moq;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Application.Services;
using SplitwiseClone.Domain.Entities;
using Xunit;

namespace SplitwiseClone.Application.Tests;

public sealed class BalanceServiceTests
{
    [Fact]
    public async Task GetGroupBalancesAsync_CalculatesBalancesAndSimplifiedTransfers()
    {
        var fixture = new Fixture();
        fixture.Group.Members.Add(new GroupMember { UserId = 1, User = new ApplicationUser { FirstName = "Ada" } });
        fixture.Group.Members.Add(new GroupMember { UserId = 2, User = new ApplicationUser { FirstName = "Bob" } });
        fixture.Group.Members.Add(new GroupMember { UserId = 3, User = new ApplicationUser { FirstName = "Cam" } });
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(9, default)).ReturnsAsync(fixture.Group);
        fixture.Expenses.Setup(repository => repository.GetByGroupIdAsync(9, default)).ReturnsAsync([
            new Expense { Splits = [new ExpenseSplit { UserId = 1, PaidShare = 100m }, new ExpenseSplit { UserId = 2, OwedShare = 50m }, new ExpenseSplit { UserId = 3, OwedShare = 50m }] }
        ]);

        var result = await fixture.Service.GetGroupBalancesAsync(9, 1);

        Assert.Equal(new[] { 100m, -50m, -50m }, result.Members.Select(member => member.NetBalance));
        Assert.Equal(2, result.Settlements.Count);
        Assert.All(result.Settlements, transfer => Assert.Equal(1, transfer.ToUserId));
        Assert.Equal(100m, result.Settlements.Sum(transfer => transfer.Amount));
    }

    [Fact]
    public async Task GetGroupBalancesAsync_RejectsNonMember()
    {
        var fixture = new Fixture();
        fixture.Group.Members.Add(new GroupMember { UserId = 1 });
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(9, default)).ReturnsAsync(fixture.Group);

        await Assert.ThrowsAsync<GroupAccessDeniedException>(() => fixture.Service.GetGroupBalancesAsync(9, 2));
        fixture.Expenses.Verify(repository => repository.GetByGroupIdAsync(9, default), Times.Never);
    }

    [Fact]
    public async Task GetGroupBalancesAsync_RejectsSplitFromRemovedMember()
    {
        var fixture = new Fixture();
        fixture.Group.Members.Add(new GroupMember { UserId = 1 });
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(9, default)).ReturnsAsync(fixture.Group);
        fixture.Expenses.Setup(repository => repository.GetByGroupIdAsync(9, default)).ReturnsAsync([
            new Expense { Splits = [new ExpenseSplit { UserId = 7, PaidShare = 1m }] }
        ]);

        await Assert.ThrowsAsync<EntityConflictException>(() => fixture.Service.GetGroupBalancesAsync(9, 1));
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public Mock<IGroupRepository> Groups { get; } = new();
        public Mock<IExpenseRepository> Expenses { get; } = new();
        public Group Group { get; } = new() { Id = 9, Name = "Home" };
        public BalanceService Service { get; }

        public Fixture()
        {
            Unit.SetupGet(unit => unit.Groups).Returns(Groups.Object);
            Unit.SetupGet(unit => unit.Expenses).Returns(Expenses.Object);
            Service = new BalanceService(Unit.Object);
        }
    }
}
