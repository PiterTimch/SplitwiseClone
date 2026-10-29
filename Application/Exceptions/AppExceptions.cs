namespace SplitwiseClone.Application.Exceptions;

using SplitwiseClone.Domain.Exceptions;

public sealed class EntityNotFoundException : NotFoundException
{
    public EntityNotFoundException(string entityName, object key) : base(entityName, key) { }
}

public sealed class EntityConflictException : ConflictException
{
    public EntityConflictException(string message) : base(message) { }
}

public sealed class UserUnauthorizedException : Domain.Exceptions.UnauthorizedAccessException
{
    public UserUnauthorizedException(string message) : base(message) { }
}
