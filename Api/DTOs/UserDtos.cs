namespace SplitwiseClone.Api.DTOs;

public record CreateUserDto(string Name, string Email);

public record UpdateUserDto(string Name, string Email);

public record UserResponseDto(Guid Id, string Name, string Email, DateTime CreatedAt);
