namespace SplitwiseClone.Domain.Entities;

using SplitwiseClone.Domain.Abstractions;

public class GroupMember : IBaseEntity
{
    public int GroupId { get; set; }
    public Group? Group { get; set; }

    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
