namespace SplitwiseClone.Application.Interfaces.Repositories;

using SplitwiseClone.Domain.Entities;

public interface IGroupMemberRepository : IBaseRepository<GroupMember>
{
    Task<GroupMember?> GetAsync(int groupId, Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GroupMember>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GroupMember>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
