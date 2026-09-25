using SplitwiseClone.Domain.Entities.Additional;
using SplitwiseClone.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace SplitwiseClone.Domain.Entities.Expense;

[Table("tbl_expense_payers")]
public class ExpensePayerEntity : BaseUniqueEntity<long>
{
    public long ExpenseId { get; set; }
    public virtual ExpenseEntity Expense { get; set; } = null!;

    public long UserId { get; set; }
    public virtual UserEntity User { get; set; } = null!;

    public decimal AmountPaid { get; set; }
}
