namespace SplitwiseClone.Application.Exceptions;

using SplitwiseClone.Domain.Exceptions;

public sealed class EntityNotFoundException(string entityName, object key)
    : NotFoundException(entityName, key)
{
}
