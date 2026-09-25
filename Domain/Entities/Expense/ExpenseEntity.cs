using SplitwiseClone.Domain.Entities.Additional;
using SplitwiseClone.Domain.Entities.Group;
using SplitwiseClone.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SplitwiseClone.Domain.Entities.Expense;

[Table("tbl_expenses")]
public class ExpenseEntity : BaseUniqueEntity<long>
{
    [StringLength(200)]
    public string Title { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public long GroupId { get; set; }
    public virtual GroupEntity Group { get; set; } = null!;

    public long CreatedById { get; set; }
    public virtual UserEntity CreatedBy { get; set; } = null!;

    public virtual ICollection<ExpensePayerEntity> Payers { get; set; } = new List<ExpensePayerEntity>();

    public virtual ICollection<ExpenseSplitEntity> Splits { get; set; } = new List<ExpenseSplitEntity>();
}
