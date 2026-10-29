namespace SplitwiseClone.Domain.Exceptions;

public abstract class UnauthorizedAccessException : DomainException
{
    protected UnauthorizedAccessException(string message) : base(message) { }
}
