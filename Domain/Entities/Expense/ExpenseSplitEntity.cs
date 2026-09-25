using SplitwiseClone.Domain.Entities.Additional;
using SplitwiseClone.Domain.Entities.Identity;

namespace SplitwiseClone.Domain.Entities.Expense;

public class ExpenseSplitEntity : BaseUniqueEntity<long>
{
    public long ExpenseId { get; set; }
    public virtual ExpenseEntity Expense { get; set; } = null!;

    public long UserId { get; set; }
    public virtual UserEntity User { get; set; } = null!;
    public decimal AmountOwed { get; set; }
}
