namespace SplitwiseClone.Infrastructure.Repositories.Implementations;

using Microsoft.EntityFrameworkCore;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Infrastructure.Data;
using SplitwiseClone.Infrastructure.Repositories.Interfaces;

public class GroupRepository : BaseRepository<Group>, IGroupRepository
{
    public GroupRepository(SplitwiseDbContext context) : base(context)
    {
    }

    public async Task<Group?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(g => g.Members)
                .ThenInclude(gm => gm.User)
            .Include(g => g.Expenses)
            .FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
    }
}
