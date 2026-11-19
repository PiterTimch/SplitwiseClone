namespace SplitwiseClone.Application.Exceptions;

using SplitwiseClone.Domain.Exceptions;

public sealed class ExpenseNotOwnedException(int expenseId)
    : ConflictException($"Only the expense creator can modify expense {expenseId}.")
{
}
