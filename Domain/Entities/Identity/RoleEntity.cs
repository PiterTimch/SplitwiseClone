using Microsoft.AspNetCore.Identity;
using SplitwiseClone.Domain.Entities.Additional;

namespace SplitwiseClone.Domain.Entities.Identity;

public class RoleEntity : IdentityRole<long>, IBaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }

    public RoleEntity() : base() { }
    public RoleEntity(string roleName) : base(roleName) { }

    public virtual ICollection<UserRoleEntity> UserRoles { get; set; } 
}
