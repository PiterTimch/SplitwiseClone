using SplitwiseClone.Domain.Entities.Additional;
using SplitwiseClone.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace SplitwiseClone.Domain.Entities.Group;

[Table("tbl_group_members")]
public class GroupMemberEntity : BaseUniqueEntity<long>
{
    public long GroupId { get; set; }
    public virtual GroupEntity Group { get; set; } = null!;

    public long UserId { get; set; }
    public virtual UserEntity User { get; set; } = null!;

    public bool IsGroupAdmin { get; set; }
}
