namespace SplitwiseClone.Application.Interfaces.Repositories;

using SplitwiseClone.Domain.Entities;

public interface IUserRepository : IBaseRepository<ApplicationUser>
{
    Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<ApplicationUser?> GetByIdWithDetailsAsync(int id, CancellationToken cancellationToken = default);
}
