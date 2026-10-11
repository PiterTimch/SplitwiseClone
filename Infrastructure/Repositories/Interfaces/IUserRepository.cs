namespace SplitwiseClone.Infrastructure.Repositories.Interfaces;

using SplitwiseClone.Domain.Entities;

public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
}
