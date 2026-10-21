namespace SplitwiseClone.Application.Interfaces.Repositories;

using SplitwiseClone.Domain.Entities;

public interface IExpenseSplitRepository : IBaseRepository<ExpenseSplit>
{
    Task<IEnumerable<ExpenseSplit>> GetByExpenseIdAsync(int expenseId, CancellationToken cancellationToken = default);
}
