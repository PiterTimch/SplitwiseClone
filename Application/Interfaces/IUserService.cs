namespace SplitwiseClone.Application.Interfaces;

using SplitwiseClone.Application.DTOs.Users;

public interface IUserService
{
    Task<UserResponseDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UserResponseDto> UpdateAsync(int id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
