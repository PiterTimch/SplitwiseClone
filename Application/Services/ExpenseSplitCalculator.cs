namespace SplitwiseClone.Application.Services;

using SplitwiseClone.Application.DTOs.Expenses;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Domain.Entities;

public static class ExpenseSplitCalculator
{
    public static IReadOnlyList<ExpenseSplit> Calculate(decimal totalCost, ExpenseSplitMode mode, IReadOnlyCollection<CreateExpenseSplitDto> inputs)
    {
        if (totalCost <= 0m || decimal.Round(totalCost, 2) != totalCost)
        {
            throw new EntityConflictException("Expense totals must be positive amounts with no more than two decimal places.");
        }

        if (inputs.Count == 0)
        {
            throw new EntityConflictException("An expense requires at least one participant.");
        }

        if (inputs.Any(input => input.PaidShare < 0m || decimal.Round(input.PaidShare, 2) != input.PaidShare || input.SplitValue < 0m))
        {
            throw new EntityConflictException("Expense shares and split values must be non-negative and paid shares must use cents.");
        }

        var values = inputs.Select(input => mode switch
        {
            ExpenseSplitMode.Equal => 1m,
            ExpenseSplitMode.Exact => input.SplitValue,
            ExpenseSplitMode.Percentage => input.SplitValue,
            ExpenseSplitMode.Shares => input.SplitValue,
            _ => throw new EntityConflictException("Unsupported expense split mode.")
        }).ToArray();

        if (mode == ExpenseSplitMode.Percentage && values.Sum() != 100m)
        {
            throw new EntityConflictException("Split percentages must sum to 100.");
        }

        decimal[] owedShares = mode switch
        {
            ExpenseSplitMode.Exact => CalculateExact(totalCost, values),
            ExpenseSplitMode.Percentage => CalculateWeighted(totalCost, values, 100m),
            ExpenseSplitMode.Shares => CalculateWeighted(totalCost, values, values.Sum()),
            ExpenseSplitMode.Equal => CalculateWeighted(totalCost, values, values.Sum()),
            _ => throw new EntityConflictException("Unsupported expense split mode.")
        };

        var paidTotal = inputs.Sum(input => input.PaidShare);
        if (paidTotal != totalCost)
        {
            throw new EntityConflictException("Paid shares must equal the expense total.");
        }

        return inputs.Select((input, index) => new ExpenseSplit
        {
            UserId = input.UserId,
            PaidShare = input.PaidShare,
            OwedShare = owedShares[index]
        }).ToList();
    }

    private static decimal[] CalculateExact(decimal totalCost, decimal[] owedShares)
    {
        if (owedShares.Any(value => decimal.Round(value, 2) != value) || owedShares.Sum() != totalCost)
        {
            throw new EntityConflictException("Exact owed shares must use cents and equal the expense total.");
        }

        return owedShares;
    }

    private static decimal[] CalculateWeighted(decimal totalCost, decimal[] weights, decimal totalWeight)
    {
        if (totalWeight <= 0m || weights.Any(weight => weight < 0m))
        {
            throw new EntityConflictException("Split weights must be non-negative and have a positive total.");
        }

        var baseShares = weights.Select(weight => decimal.Floor(totalCost * weight / totalWeight * 100m) / 100m).ToArray();
        var centsRemaining = decimal.ToInt64((totalCost - baseShares.Sum()) * 100m);
        for (var index = 0; centsRemaining > 0; index = (index + 1) % baseShares.Length)
        {
            if (weights[index] == 0m)
            {
                continue;
            }

            baseShares[index] += 0.01m;
            centsRemaining--;
        }

        return baseShares;
    }
}
