namespace SplitwiseClone.Domain.Exceptions;

public abstract class NotFoundException : DomainException
{
    protected NotFoundException(string message) : base(message) { }

    protected NotFoundException(string entityName, object key)
        : base($"Entity '{entityName}' with key '{key}' was not found.") { }
}
