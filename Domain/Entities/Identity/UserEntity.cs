using Microsoft.AspNetCore.Identity;
using SplitwiseClone.Domain.Entities.Additional;

namespace SplitwiseClone.Domain.Entities.Identity;

public class UserEntity : IdentityUser<long>
{
    public string? FirstName { get; set; } = null;
    public string? LastName { get; set; } = null;

    public DateTime DateCreated { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
    public bool IsDeleted { get; set; }
}
