namespace SplitwiseClone.Application.Interfaces;

using Microsoft.AspNetCore.Identity;

public interface IJwtTokenService
{
    string GenerateToken(IdentityUser<Guid> user, IList<string> roles);
}
