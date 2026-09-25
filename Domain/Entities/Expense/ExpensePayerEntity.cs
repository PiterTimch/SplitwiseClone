using SplitwiseClone.Domain.Entities.Additional;
using SplitwiseClone.Domain.Entities.Identity;

namespace SplitwiseClone.Domain.Entities.Expense;

public class ExpensePayerEntity : BaseUniqueEntity<long>
{
    public long ExpenseId { get; set; }
    public virtual ExpenseEntity Expense { get; set; } = null!;

    public long UserId { get; set; }
    public virtual UserEntity User { get; set; } = null!;

    public decimal AmountPaid { get; set; }
}
