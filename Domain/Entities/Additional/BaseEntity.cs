using System.ComponentModel.DataAnnotations;

namespace SplitwiseClone.Domain.Entities.Additional;

public class BaseEntity<Tid>
{
    [Key]
    public Tid Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
    public bool IsDeleted { get; set; }
}
