namespace SplitwiseClone.Application.Services;

using SplitwiseClone.Application.DTOs.Users;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;
using SplitwiseClone.Domain.Entities;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<ApplicationUser> _userManager;

    public UserService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
    }

    public async Task<UserResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("User", id);
        return Map(user);
    }

    public async Task<UserResponseDto> UpdateAsync(int id, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("User", id);

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Email = dto.Email;
        user.UserName = dto.Email;
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new EntityConflictException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }

        return Map(user);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _unitOfWork.Users.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("User", id);
        if (user.GroupMembers.Count > 0 || user.ExpensesCreated.Count > 0 || user.ExpenseSplits.Count > 0)
        {
            throw new EntityConflictException("A user with group or expense history cannot be deleted.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new EntityConflictException(string.Join("; ", result.Errors.Select(error => error.Description)));
        }
    }

    private static UserResponseDto Map(ApplicationUser user) =>
        new(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, user.CreatedAt);
}
