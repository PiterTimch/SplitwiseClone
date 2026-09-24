using Microsoft.AspNetCore.Identity;

namespace SplitwiseClone.Domain.Entities.Identity;

public class UserRoleEntity : IdentityUserRole<long>
{
    public virtual UserEntity User { get; set; } = null!;
    public virtual RoleEntity Role { get; set; } = null!;
}
