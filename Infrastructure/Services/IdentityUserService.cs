namespace SplitwiseClone.Infrastructure.Services;

using Microsoft.AspNetCore.Identity;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Domain.Entities;

public sealed class IdentityUserService(UserManager<ApplicationUser> userManager) : IIdentityUserService
{
    public Task<ApplicationUser?> FindByEmailAsync(string email) => userManager.FindByEmailAsync(email);

    public Task<bool> CheckPasswordAsync(ApplicationUser user, string password) => userManager.CheckPasswordAsync(user, password);

    public async Task<IReadOnlyList<string>> GetRolesAsync(ApplicationUser user) => (await userManager.GetRolesAsync(user)).ToArray();

    public async Task<IdentityOperationResult> CreateAsync(ApplicationUser user, string password) => Map(await userManager.CreateAsync(user, password));

    public async Task<IdentityOperationResult> UpdateAsync(ApplicationUser user) => Map(await userManager.UpdateAsync(user));

    public async Task<IdentityOperationResult> DeleteAsync(ApplicationUser user) => Map(await userManager.DeleteAsync(user));

    private static IdentityOperationResult Map(IdentityResult result) =>
        new(result.Succeeded, result.Errors.Select(error => error.Description).ToArray());
}
