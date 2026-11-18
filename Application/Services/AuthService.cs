namespace SplitwiseClone.Application.Services;

using SplitwiseClone.Application.DTOs.Auth;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Domain.Entities;

public class AuthService : IAuthService
{
    private readonly IIdentityUserService _identityUserService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        IIdentityUserService identityUserService,
        IJwtTokenService jwtTokenService)
    {
        _identityUserService = identityUserService;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _identityUserService.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            throw new EntityConflictException("ApplicationUser with this email already exists.");
        }

        var appUser = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _identityUserService.CreateAsync(appUser, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors);
            throw new EntityConflictException($"Registration failed: {errors}");
        }

        var roles = await _identityUserService.GetRolesAsync(appUser);
        var token = _jwtTokenService.GenerateToken(appUser, roles.ToList());

        return new AuthResponseDto(token, appUser.Email!, appUser.FullName, appUser.Id);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var appUser = await _identityUserService.FindByEmailAsync(dto.Email);
        if (appUser is null)
        {
            throw new UserUnauthorizedException("Invalid email or password.");
        }

        var isPasswordValid = await _identityUserService.CheckPasswordAsync(appUser, dto.Password);
        if (!isPasswordValid)
        {
            throw new UserUnauthorizedException("Invalid email or password.");
        }

        var roles = await _identityUserService.GetRolesAsync(appUser);
        var token = _jwtTokenService.GenerateToken(appUser, roles.ToList());

        return new AuthResponseDto(token, appUser.Email!, appUser.FullName, appUser.Id);
    }
}
