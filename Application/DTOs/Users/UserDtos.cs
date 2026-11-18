namespace SplitwiseClone.Application.DTOs.Users;

public record UpdateUserDto(string FirstName, string LastName, string Email);

public record UserResponseDto(int Id, string FirstName, string LastName, string Email, DateTime CreatedAt);
