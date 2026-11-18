namespace SplitwiseClone.Application.DTOs.Auth;

public record RegisterRequestDto(string FirstName, string LastName, string Email, string Password);

public record LoginRequestDto(string Email, string Password);

public record AuthResponseDto(string AccessToken, string Email, string Name, int UserId);
