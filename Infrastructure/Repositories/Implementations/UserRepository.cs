namespace SplitwiseClone.Infrastructure.Repositories.Implementations;

using Microsoft.EntityFrameworkCore;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Infrastructure.Data;
using SplitwiseClone.Application.Interfaces.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(SplitwiseDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<User?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(u => u.GroupMembers)
                .ThenInclude(gm => gm.Group)
            .Include(u => u.ExpensesCreated)
            .Include(u => u.ExpenseSplits)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
    }
}
