namespace SplitwiseClone.Infrastructure.Repositories.Implementations;

using Microsoft.EntityFrameworkCore;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Infrastructure.Data;
using SplitwiseClone.Application.Interfaces.Repositories;

public class ExpenseRepository : BaseRepository<Expense>, IExpenseRepository
{
    public ExpenseRepository(SplitwiseDbContext context) : base(context)
    {
    }

    public async Task<Expense?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Group)
            .Include(e => e.CreatedByUser)
            .Include(e => e.Splits)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Expense>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(e => e.Group)
            .Include(e => e.CreatedByUser)
            .Include(e => e.Splits)
                .ThenInclude(s => s.User)
            .Where(e => e.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Expense>> GetUngroupedByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(expense => expense.CreatedByUser)
            .Include(expense => expense.Splits)
                .ThenInclude(split => split.User)
            .Where(expense => expense.GroupId == null &&
                (expense.CreatedByUserId == userId || expense.Splits.Any(split => split.UserId == userId)))
            .ToListAsync(cancellationToken);
    }

    public async Task<Expense?> GetByIdWithSplitsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(expense => expense.Splits)
            .FirstOrDefaultAsync(expense => expense.Id == id, cancellationToken);
    }
}
