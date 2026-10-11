namespace SplitwiseClone.Infrastructure.Repositories.Interfaces;

using SplitwiseClone.Domain.Entities;

public interface IExpenseRepository : IBaseRepository<Expense>
{
    Task<Expense?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Expense>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);
}
