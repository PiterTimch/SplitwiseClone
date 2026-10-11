namespace SplitwiseClone.Domain.Abstractions;

public interface IEntity : IBaseEntity
{
}

public interface IEntity<TKey> : IEntity
{
    TKey Id { get; set; }
}
