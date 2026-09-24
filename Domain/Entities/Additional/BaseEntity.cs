using System.ComponentModel.DataAnnotations;

namespace SplitwiseClone.Domain.Entities.Additional;

public interface IBaseEntity
{
    DateTime CreatedAt { get; set; }
    bool IsDeleted { get; set; }
}

public abstract class BaseUniqueEntity<Tid> : IBaseEntity
{
    [Key]
    public Tid Id { get; set; } = default!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
}
