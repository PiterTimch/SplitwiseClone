namespace SplitwiseClone.Application.Interfaces.Repositories;

using SplitwiseClone.Domain.Entities;

public interface IGroupMemberRepository : IBaseRepository<GroupMember>
{
    Task<GroupMember?> GetAsync(int groupId, int userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GroupMember>> GetByGroupIdAsync(int groupId, CancellationToken cancellationToken = default);
    Task<IEnumerable<GroupMember>> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
}
