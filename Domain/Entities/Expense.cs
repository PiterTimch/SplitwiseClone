namespace SplitwiseClone.Domain.Entities;

using SplitwiseClone.Domain.Abstractions;

public class Expense : IEntity<int>
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public double TotalAmount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public int? GroupId { get; set; }
    public Group? Group { get; set; }
    public Guid CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
    public bool IsPayment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ExpenseSplit> Splits { get; set; } = new List<ExpenseSplit>();
}
