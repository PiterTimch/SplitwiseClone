using SplitwiseClone.Domain.Entities.Additional;
using SplitwiseClone.Domain.Entities.Identity;

namespace SplitwiseClone.Domain.Entities.Group;

public class GroupMemberEntity : BaseUniqueEntity<long>
{
    public long GroupId { get; set; }
    public virtual GroupEntity Group { get; set; } = null!;

    public long UserId { get; set; }
    public virtual UserEntity User { get; set; } = null!;

    public bool IsGroupAdmin { get; set; }
}
