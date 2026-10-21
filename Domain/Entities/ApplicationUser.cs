namespace SplitwiseClone.Domain.Entities;

using Microsoft.AspNetCore.Identity;

/// <summary>
/// Application user that extends ASP.NET Core Identity's IdentityUser.
/// The Guid Id links to the domain User entity (same Guid).
/// </summary>
public class ApplicationUser : IdentityUser<Guid>
{
    public string DisplayName { get; set; } = string.Empty;
}
