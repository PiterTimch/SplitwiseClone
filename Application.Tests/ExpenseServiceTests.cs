using Moq;
using SplitwiseClone.Application.DTOs.Expenses;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Application.Services;
using SplitwiseClone.Domain.Entities;
using Xunit;

namespace SplitwiseClone.Application.Tests;

public sealed class ExpenseServiceTests
{
    [Fact]
    public async Task CreateAsync_RejectsNonMemberSplit()
    {
        var fixture = new Fixture();
        fixture.Group.Members.Add(new GroupMember { GroupId = 10, UserId = 1 });
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(10, default)).ReturnsAsync(fixture.Group);
        var request = new CreateExpenseDto("Dinner", 10m, null, 10, false, ExpenseSplitMode.Equal, [new(2, 10m, 0m)]);
        await Assert.ThrowsAsync<GroupAccessDeniedException>(() => fixture.Service.CreateAsync(1, request));
        fixture.Expenses.Verify(repository => repository.AddAsync(It.IsAny<Expense>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_StoresCalculatedSplitsAndCreator()
    {
        var fixture = new Fixture();
        fixture.Group.Members.Add(new GroupMember { GroupId = 10, UserId = 1 });
        fixture.Group.Members.Add(new GroupMember { GroupId = 10, UserId = 2 });
        fixture.Group.Members.Add(new GroupMember { GroupId = 10, UserId = 3 });
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(10, default)).ReturnsAsync(fixture.Group);
        fixture.Expenses.Setup(repository => repository.AddAsync(It.IsAny<Expense>(), default))
            .Callback<Expense, CancellationToken>((expense, _) => expense.Id = 5).Returns(Task.CompletedTask);
        fixture.Expenses.Setup(repository => repository.GetByIdWithDetailsAsync(5, default)).ReturnsAsync((Expense?)null);
        var request = new CreateExpenseDto("Dinner", 10m, null, 10, false, ExpenseSplitMode.Equal,
            [new(1, 10m, 0m), new(2, 0m, 0m), new(3, 0m, 0m)]);
        var result = await fixture.Service.CreateAsync(1, request);
        Assert.Equal(1, result.CreatedByUserId);
        Assert.Equal(new[] { 3.34m, 3.33m, 3.33m }, result.Splits.Select(split => split.OwedShare));
        fixture.Unit.Verify(unit => unit.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task GetByGroupAsync_RejectsNonMemberBeforeReadingExpenses()
    {
        var fixture = new Fixture();
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(10, default)).ReturnsAsync(fixture.Group);
        await Assert.ThrowsAsync<GroupAccessDeniedException>(() => fixture.Service.GetByGroupAsync(10, 7));
        fixture.Expenses.Verify(repository => repository.GetByGroupIdAsync(10, default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_RejectsAnotherCreatorsExpense()
    {
        var fixture = new Fixture();
        var expense = new Expense { Id = 5, GroupId = 10, CreatedByUserId = 2, TotalCost = 10m };
        fixture.Group.Members.Add(new GroupMember { GroupId = 10, UserId = 1 });
        fixture.Groups.Setup(repository => repository.GetByIdWithDetailsAsync(10, default)).ReturnsAsync(fixture.Group);
        fixture.Expenses.Setup(repository => repository.GetByIdWithSplitsAsync(5, default)).ReturnsAsync(expense);
        var request = new UpdateExpenseDto("Changed", 10m, DateTime.UtcNow, false, ExpenseSplitMode.Equal, [new(1, 10m, 0m)]);
        await Assert.ThrowsAsync<GroupAccessDeniedException>(() => fixture.Service.UpdateAsync(5, 1, request));
        fixture.Unit.Verify(unit => unit.SaveChangesAsync(default), Times.Never);
    }

    private sealed class Fixture
    {
        public Mock<IUnitOfWork> Unit { get; } = new();
        public Mock<IExpenseRepository> Expenses { get; } = new();
        public Mock<IGroupRepository> Groups { get; } = new();
        public Group Group { get; } = new() { Id = 10, Name = "Trip" };
        public ExpenseService Service { get; }

        public Fixture()
        {
            Unit.SetupGet(unit => unit.Expenses).Returns(Expenses.Object);
            Unit.SetupGet(unit => unit.Groups).Returns(Groups.Object);
            Service = new ExpenseService(Unit.Object);
        }
    }
}
