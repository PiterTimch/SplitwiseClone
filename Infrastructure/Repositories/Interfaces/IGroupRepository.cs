namespace SplitwiseClone.Infrastructure.Repositories.Interfaces;

using SplitwiseClone.Domain.Entities;

public interface IGroupRepository : IBaseRepository<Group>
{
    Task<Group?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
