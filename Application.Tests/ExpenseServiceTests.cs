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

public class ExpenseServiceTests
{
    private readonly Mock<IUnitOfWork> _work = new();
    private readonly Mock<IExpenseRepository> _expenses = new();
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IGroupRepository> _groups = new();
    private readonly ExpenseService _service;

    public ExpenseServiceTests()
    {
        _work.SetupGet(x => x.Expenses).Returns(_expenses.Object);
        _work.SetupGet(x => x.Users).Returns(_users.Object);
        _work.SetupGet(x => x.Groups).Returns(_groups.Object);
        _service = new ExpenseService(_work.Object);
    }

    [Fact]
    public async Task GetAllAsync_SkipsMissingDetailsAndMapsFoundExpenses()
    {
        _expenses.Setup(x => x.GetAllAsync(default)).ReturnsAsync(new[] { new Expense { Id = 1 }, new Expense { Id = 2 } });
        _expenses.Setup(x => x.GetByIdWithDetailsAsync(1, default)).ReturnsAsync((Expense?)null);
        _expenses.Setup(x => x.GetByIdWithDetailsAsync(2, default)).ReturnsAsync(new Expense { Id = 2, Description = "Meal" });

        var result = (await _service.GetAllAsync()).ToList();

        Assert.Single(result);
        Assert.Equal("Meal", result[0].Description);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task GetByIdAsync_ReturnsMappedExpenseOrNull(bool exists)
    {
        _expenses.Setup(x => x.GetByIdWithDetailsAsync(1, default)).ReturnsAsync(exists ? new Expense { Id = 1, Description = "Meal" } : null);

        var result = await _service.GetByIdAsync(1);

        if (exists) Assert.Equal("Meal", result?.Description);
        else Assert.Null(result);
    }

    [Fact]
    public async Task GetByGroupAsync_MapsExpenses()
    {
        _expenses.Setup(x => x.GetByGroupIdAsync(4, default)).ReturnsAsync(new[] { new Expense { Id = 2, Description = "Meal" } });

        var result = (await _service.GetByGroupAsync(4)).Single();

        Assert.Equal(2, result.Id);
        Assert.Equal("Meal", result.Description);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateAsync_CreatesExpenseWithOptionalGroupAndSplits(bool withGroup)
    {
        var userId = Guid.NewGuid();
        var date = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        _users.Setup(x => x.GetByIdAsync(userId, default)).ReturnsAsync(new User { Id = userId });
        if (withGroup) _groups.Setup(x => x.GetByIdAsync(5, default)).ReturnsAsync(new Group { Id = 5, Name = "Trip" });
        _expenses.Setup(x => x.AddAsync(It.IsAny<Expense>(), default)).Callback<Expense, CancellationToken>((e, _) => e.Id = 7).Returns(Task.CompletedTask);
        _expenses.Setup(x => x.GetByIdWithDetailsAsync(7, default)).ReturnsAsync((Expense?)null);

        var result = await _service.CreateAsync(userId, new("Meal", 50, date, withGroup ? 5 : null, false,
            new() { new(Guid.NewGuid(), 10, 25), new(Guid.NewGuid(), 40, 25) }));

        Assert.Equal(7, result.Id);
        Assert.Equal("Meal", result.Description);
        Assert.Equal(50, result.TotalAmount);
        Assert.Equal(date, result.Date);
        Assert.Equal(withGroup ? 5 : null, result.GroupId);
        Assert.Equal(2, result.Splits.Count);
        Assert.Equal(10, result.Splits[0].PaidAmount);
        Assert.Equal(25, result.Splits[1].OwedAmount);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_AllowsEmptySplitsAndUsesFallbackDate()
    {
        var userId = Guid.NewGuid();
        Expense? added = null;
        _users.Setup(x => x.GetByIdAsync(userId, default)).ReturnsAsync(new User { Id = userId });
        _expenses.Setup(x => x.AddAsync(It.IsAny<Expense>(), default)).Callback<Expense, CancellationToken>((e, _) => added = e).Returns(Task.CompletedTask);
        _expenses.Setup(x => x.GetByIdWithDetailsAsync(It.IsAny<int>(), default)).ReturnsAsync((Expense?)null);

        var before = DateTime.UtcNow;
        var result = await _service.CreateAsync(userId, new("Meal", 1, null, null, true, null));
        var after = DateTime.UtcNow;

        Assert.Empty(result.Splits);
        Assert.True(added!.Date >= before && added.Date <= after);
        Assert.True(added.IsPayment);
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenCreatorMissing()
    {
        var userId = Guid.NewGuid();
        _users.Setup(x => x.GetByIdAsync(userId, default)).ReturnsAsync((User?)null);

        await Assert.ThrowsAsync<EntityConflictException>(() => _service.CreateAsync(userId, new("Meal", 1, null, null, false, null)));

        _expenses.Verify(x => x.AddAsync(It.IsAny<Expense>(), default), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ThrowsWhenGroupMissing()
    {
        var userId = Guid.NewGuid();
        _users.Setup(x => x.GetByIdAsync(userId, default)).ReturnsAsync(new User { Id = userId });
        _groups.Setup(x => x.GetByIdAsync(5, default)).ReturnsAsync((Group?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.CreateAsync(userId, new("Meal", 1, null, 5, false, null)));

        _expenses.Verify(x => x.AddAsync(It.IsAny<Expense>(), default), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesExpense()
    {
        var expense = new Expense { Id = 3, Description = "Old" };
        var date = DateTime.UtcNow;
        _expenses.Setup(x => x.GetByIdAsync(3, default)).ReturnsAsync(expense);

        await _service.UpdateAsync(3, new("New", 20, date, true));

        Assert.Equal("New", expense.Description);
        Assert.Equal(20, expense.TotalAmount);
        Assert.Equal(date, expense.Date);
        Assert.True(expense.IsPayment);
        _expenses.Verify(x => x.Update(expense), Times.Once);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ThrowsWhenExpenseMissing()
    {
        _expenses.Setup(x => x.GetByIdAsync(3, default)).ReturnsAsync((Expense?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.UpdateAsync(3, new("New", 1, DateTime.UtcNow, false)));

        _work.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_DeletesExpense()
    {
        var expense = new Expense { Id = 3 };
        _expenses.Setup(x => x.GetByIdAsync(3, default)).ReturnsAsync(expense);

        await _service.DeleteAsync(3);

        _expenses.Verify(x => x.Delete(expense), Times.Once);
        _work.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ThrowsWhenExpenseMissing()
    {
        _expenses.Setup(x => x.GetByIdAsync(3, default)).ReturnsAsync((Expense?)null);

        await Assert.ThrowsAsync<EntityNotFoundException>(() => _service.DeleteAsync(3));

        _work.Verify(x => x.SaveChangesAsync(default), Times.Never);
    }
}

