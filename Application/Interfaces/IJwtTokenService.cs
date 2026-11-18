namespace SplitwiseClone.Application.Interfaces;

using Microsoft.AspNetCore.Identity;

public interface IJwtTokenService
{
    string GenerateToken(IdentityUser<int> user, IList<string> roles);
}
