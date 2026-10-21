namespace SplitwiseClone.Application.DTOs.Auth;

public record RegisterRequestDto(string Name, string Email, string Password);

public record LoginRequestDto(string Email, string Password);

public record AuthResponseDto(string AccessToken, string Email, string Name, Guid UserId);
