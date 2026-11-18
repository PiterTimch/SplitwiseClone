namespace SplitwiseClone.Application.Services;

using SplitwiseClone.Application.DTOs.Users;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Domain.Entities;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIdentityUserService _identityUserService;

    public UserService(IUnitOfWork unitOfWork, IIdentityUserService identityUserService)
    {
        _unitOfWork = unitOfWork;
        _identityUserService = identityUserService;
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
        var result = await _identityUserService.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new EntityConflictException(string.Join("; ", result.Errors));
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

        var result = await _identityUserService.DeleteAsync(user);
        if (!result.Succeeded)
        {
            throw new EntityConflictException(string.Join("; ", result.Errors));
        }
    }

    private static UserResponseDto Map(ApplicationUser user) =>
        new(user.Id, user.FirstName, user.LastName, user.Email ?? string.Empty, user.CreatedAt);
}
