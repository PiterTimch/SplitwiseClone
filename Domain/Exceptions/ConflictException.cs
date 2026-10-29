namespace SplitwiseClone.Domain.Exceptions;

public abstract class ConflictException : DomainException
{
    protected ConflictException(string message) : base(message) { }
}
