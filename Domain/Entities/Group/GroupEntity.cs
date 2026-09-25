using SplitwiseClone.Domain.Entities.Additional;
using SplitwiseClone.Domain.Entities.Expense;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SplitwiseClone.Domain.Entities.Group;

[Table("tbl_groups")]
public class GroupEntity : BaseUniqueEntity<long>
{
    [StringLength(100)]
    public string Name { get; set; } = null!;

    public virtual ICollection<GroupMemberEntity> Members { get; set; } = new List<GroupMemberEntity>();
    public virtual ICollection<ExpenseEntity> Expenses { get; set; } = new List<ExpenseEntity>();
}
