using SplitwiseClone.Application.DTOs.Expenses;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Services;
using Xunit;

namespace SplitwiseClone.Application.Tests;

public sealed class ExpenseSplitCalculatorTests
{
    [Fact]
    public void EqualSplit_DistributesRemainderCentsWithoutLosingMoney()
    {
        var inputs = new[] { new CreateExpenseSplitDto(1, 10m, 0m), new CreateExpenseSplitDto(2, 0m, 0m), new CreateExpenseSplitDto(3, 0m, 0m) };
        var result = ExpenseSplitCalculator.Calculate(10m, ExpenseSplitMode.Equal, inputs);
        Assert.Equal(new[] { 3.34m, 3.33m, 3.33m }, result.Select(split => split.OwedShare));
        Assert.Equal(10m, result.Sum(split => split.OwedShare));
    }

    [Fact]
    public void EqualSplit_DistributesSingleCentToOneParticipant()
    {
        var inputs = new[] { new CreateExpenseSplitDto(1, 0.01m, 0m), new CreateExpenseSplitDto(2, 0m, 0m), new CreateExpenseSplitDto(3, 0m, 0m) };

        var result = ExpenseSplitCalculator.Calculate(0.01m, ExpenseSplitMode.Equal, inputs);

        Assert.Equal(0.01m, result.Sum(split => split.OwedShare));
        Assert.Equal(1, result.Count(split => split.OwedShare == 0.01m));
        Assert.All(result.Where(split => split.OwedShare == 0m), split => Assert.Equal(0m, split.OwedShare));
    }

    [Theory]
    [InlineData(ExpenseSplitMode.Exact, 60, 40, 60, 40)]
    [InlineData(ExpenseSplitMode.Percentage, 60, 40, 60, 40)]
    [InlineData(ExpenseSplitMode.Shares, 3, 2, 60, 40)]
    public void SplitModes_CalculateExpectedAmounts(ExpenseSplitMode mode, decimal first, decimal second, decimal expectedFirst, decimal expectedSecond)
    {
        var inputs = new[] { new CreateExpenseSplitDto(1, 100m, first), new CreateExpenseSplitDto(2, 0m, second) };
        var result = ExpenseSplitCalculator.Calculate(100m, mode, inputs);
        Assert.Equal(new[] { expectedFirst, expectedSecond }, result.Select(split => split.OwedShare));
    }

    [Fact]
    public void Calculate_RejectsUnbalancedPayments()
    {
        var inputs = new[] { new CreateExpenseSplitDto(1, 99m, 100m) };
        Assert.Throws<EntityConflictException>(() => ExpenseSplitCalculator.Calculate(100m, ExpenseSplitMode.Exact, inputs));
    }

    [Fact]
    public void Calculate_RejectsPercentagesThatDoNotSumToOneHundred()
    {
        var inputs = new[] { new CreateExpenseSplitDto(1, 100m, 99m) };
        Assert.Throws<EntityConflictException>(() => ExpenseSplitCalculator.Calculate(100m, ExpenseSplitMode.Percentage, inputs));
    }

    [Fact]
    public void Calculate_RejectsExactSharesWithFractionsOfCent()
    {
        var inputs = new[] { new CreateExpenseSplitDto(1, 1m, 0.999m) };
        Assert.Throws<EntityConflictException>(() => ExpenseSplitCalculator.Calculate(1m, ExpenseSplitMode.Exact, inputs));
    }

    [Fact]
    public void Calculate_RejectsInvalidTotalOrParticipantAmounts()
    {
        var invalidCases = new[]
        {
            (Total: 0m, PaidShare: 0m),
            (Total: -1m, PaidShare: 0m),
            (Total: 1m, PaidShare: -0.01m),
            (Total: 1m, PaidShare: 0.001m)
        };

        foreach (var (total, paidShare) in invalidCases)
        {
            var inputs = new[] { new CreateExpenseSplitDto(1, paidShare, 1m) };
            Assert.Throws<EntityConflictException>(() => ExpenseSplitCalculator.Calculate(total, ExpenseSplitMode.Exact, inputs));
        }
    }
}
