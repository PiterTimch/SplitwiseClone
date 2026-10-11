namespace SplitwiseClone.Domain.Entities;

using SplitwiseClone.Domain.Abstractions;

public class GroupMember : IBaseEntity
{
    public int GroupId { get; set; }
    public Group? Group { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
