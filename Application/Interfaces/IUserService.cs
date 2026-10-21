namespace SplitwiseClone.Application.Interfaces;

using SplitwiseClone.Application.DTOs.Users;

public interface IUserService
{
    Task<IEnumerable<UserResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserResponseDto> UpdateAsync(Guid id, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
