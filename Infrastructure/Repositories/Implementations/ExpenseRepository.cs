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
            .Include(e => e.CreatedByUser)
            .Include(e => e.Splits)
                .ThenInclude(s => s.User)
            .Where(e => e.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }
}
