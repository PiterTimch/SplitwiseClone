namespace SplitwiseClone.Application.Exceptions;

public sealed class UserUnauthorizedException(string message)
    : SplitwiseClone.Domain.Exceptions.UnauthorizedAccessException(message)
{
}
