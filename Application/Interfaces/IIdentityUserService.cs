namespace SplitwiseClone.Application.Interfaces;

using SplitwiseClone.Domain.Entities;

public interface IIdentityUserService
{
    Task<ApplicationUser?> FindByEmailAsync(string email);
    Task<bool> CheckPasswordAsync(ApplicationUser user, string password);
    Task<IReadOnlyList<string>> GetRolesAsync(ApplicationUser user);
    Task<IdentityOperationResult> CreateAsync(ApplicationUser user, string password);
    Task<IdentityOperationResult> UpdateAsync(ApplicationUser user);
    Task<IdentityOperationResult> DeleteAsync(ApplicationUser user);
}

public record IdentityOperationResult(bool Succeeded, IReadOnlyList<string> Errors);
