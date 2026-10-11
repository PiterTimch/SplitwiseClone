namespace SplitwiseClone.Infrastructure.Repositories.Interfaces;

using SplitwiseClone.Domain.Entities;

public interface IExpenseSplitRepository : IBaseRepository<ExpenseSplit>
{
    Task<IEnumerable<ExpenseSplit>> GetByExpenseIdAsync(int expenseId, CancellationToken cancellationToken = default);
}
