namespace SplitwiseClone.Application.Services;

using Microsoft.AspNetCore.Identity;
using SplitwiseClone.Application.DTOs.Auth;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Domain.Entities;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
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

        var result = await _userManager.CreateAsync(appUser, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new EntityConflictException($"Registration failed: {errors}");
        }

        var roles = await _userManager.GetRolesAsync(appUser);
        var token = _jwtTokenService.GenerateToken(appUser, roles);

        return new AuthResponseDto(token, appUser.Email!, appUser.FullName, appUser.Id);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var appUser = await _userManager.FindByEmailAsync(dto.Email);
        if (appUser is null)
        {
            throw new UserUnauthorizedException("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(appUser, dto.Password);
        if (!isPasswordValid)
        {
            throw new UserUnauthorizedException("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(appUser);
        var token = _jwtTokenService.GenerateToken(appUser, roles);

        return new AuthResponseDto(token, appUser.Email!, appUser.FullName, appUser.Id);
    }
}
