using Microsoft.AspNetCore.Identity;
using SplitwiseClone.Domain.Entities.Additional;
using System.ComponentModel.DataAnnotations;

namespace SplitwiseClone.Domain.Entities.Identity;

public class UserEntity : IdentityUser<long>, IBaseEntity
{
    [StringLength(100)]
    public string? FirstName { get; set; }
    [StringLength(100)]
    public string? LastName { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public virtual ICollection<UserRoleEntity>? UserRoles { get; set; }
}
