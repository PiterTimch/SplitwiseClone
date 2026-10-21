namespace SplitwiseClone.Infrastructure.Repositories.Implementations;

using Microsoft.EntityFrameworkCore;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Infrastructure.Data;
using SplitwiseClone.Application.Interfaces.Repositories;

public class ExpenseSplitRepository : BaseRepository<ExpenseSplit>, IExpenseSplitRepository
{
    public ExpenseSplitRepository(SplitwiseDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExpenseSplit>> GetByExpenseIdAsync(int expenseId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(s => s.User)
            .Where(s => s.ExpenseId == expenseId)
            .ToListAsync(cancellationToken);
    }
}
