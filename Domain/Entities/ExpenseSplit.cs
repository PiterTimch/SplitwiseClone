namespace SplitwiseClone.Domain.Entities;

using SplitwiseClone.Domain.Abstractions;

public class ExpenseSplit : IEntity<int>
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    public Expense? Expense { get; set; }
    public Guid UserId { get; set; }
    public User? User { get; set; }
    public double PaidAmount { get; set; }
    public double OwedAmount { get; set; }
}
