namespace SplitwiseClone.Application.Interfaces.Repositories;

using SplitwiseClone.Domain.Entities;

public interface IGroupRepository : IBaseRepository<Group>
{
    Task<Group?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
