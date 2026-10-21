namespace SplitwiseClone.Application.DTOs.Users;

public record UpdateUserDto(string Name, string Email);

public record UserResponseDto(Guid Id, string Name, string Email, DateTime CreatedAt);
