namespace SplitwiseClone.Infrastructure.Repositories.Implementations;

using Microsoft.EntityFrameworkCore;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Infrastructure.Data;
using SplitwiseClone.Infrastructure.Repositories.Interfaces;

public class GroupMemberRepository : BaseRepository<GroupMember>, IGroupMemberRepository
{
    public GroupMemberRepository(SplitwiseDbContext context) : base(context)
    {
    }

    public async Task<GroupMember?> GetAsync(int groupId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(gm => gm.Group)
            .Include(gm => gm.User)
            .FirstOrDefaultAsync(gm => gm.GroupId == groupId && gm.UserId == userId, cancellationToken);
    }

    public async Task<IEnumerable<GroupMember>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(gm => gm.User)
            .Where(gm => gm.GroupId == groupId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<GroupMember>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(gm => gm.Group)
            .Where(gm => gm.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
