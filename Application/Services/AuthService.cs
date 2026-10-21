namespace SplitwiseClone.Application.Services;

using Microsoft.AspNetCore.Identity;
using SplitwiseClone.Application.DTOs.Auth;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Application.Interfaces.Repositories;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto dto, CancellationToken cancellationToken = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser is not null)
        {
            throw new ConflictException("User with this email already exists.");
        }

        var userId = Guid.NewGuid();

        var appUser = new ApplicationUser
        {
            Id = userId,
            UserName = dto.Email,
            Email = dto.Email,
            DisplayName = dto.Name
        };

        var result = await _userManager.CreateAsync(appUser, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new BadRequestException($"Registration failed: {errors}");
        }

        // Create mirrored domain User entity (same Guid)
        var domainUser = new User
        {
            Id = userId,
            Name = dto.Name,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(domainUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var roles = await _userManager.GetRolesAsync(appUser);
        var token = _jwtTokenService.GenerateToken(appUser, roles);

        return new AuthResponseDto(token, appUser.Email!, appUser.DisplayName, appUser.Id);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        var appUser = await _userManager.FindByEmailAsync(dto.Email);
        if (appUser is null)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(appUser, dto.Password);
        if (!isPasswordValid)
        {
            throw new UnauthorizedException("Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(appUser);
        var token = _jwtTokenService.GenerateToken(appUser, roles);

        return new AuthResponseDto(token, appUser.Email!, appUser.DisplayName, appUser.Id);
    }
}
