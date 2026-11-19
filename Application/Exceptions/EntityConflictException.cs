namespace SplitwiseClone.Application.Exceptions;

using SplitwiseClone.Domain.Exceptions;

public sealed class EntityConflictException(string message) : ConflictException(message)
{
}
