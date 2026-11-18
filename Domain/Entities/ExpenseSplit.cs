namespace SplitwiseClone.Domain.Entities;

using SplitwiseClone.Domain.Abstractions;

public class ExpenseSplit : IEntity<int>
{
    public int Id { get; set; }
    public int ExpenseId { get; set; }
    public Expense? Expense { get; set; }
    public int UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public decimal PaidShare { get; set; }
    public decimal OwedShare { get; set; }
}
