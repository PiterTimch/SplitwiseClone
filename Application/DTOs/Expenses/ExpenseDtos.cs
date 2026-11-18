namespace SplitwiseClone.Application.DTOs.Expenses;

public enum ExpenseSplitMode
{
    Equal,
    Exact,
    Percentage,
    Shares
}

public record CreateExpenseSplitDto(int UserId, decimal PaidShare, decimal SplitValue);

public record CreateExpenseDto(
    string Description,
    decimal TotalCost,
    DateTime? Date,
    int GroupId,
    bool IsPayment,
    ExpenseSplitMode SplitMode,
    List<CreateExpenseSplitDto>? Splits);

public record UpdateExpenseDto(
    string Description,
    decimal TotalCost,
    DateTime Date,
    bool IsPayment,
    ExpenseSplitMode SplitMode,
    List<CreateExpenseSplitDto> Splits);

public record ExpenseSplitResponseDto(int Id, int UserId, string? UserName, decimal PaidShare, decimal OwedShare);

public record ExpenseResponseDto(
    int Id,
    string Description,
    decimal TotalCost,
    DateTime Date,
    int GroupId,
    string? GroupName,
    int CreatedByUserId,
    string? CreatedByUserName,
    bool IsPayment,
    DateTime CreatedAt,
    List<ExpenseSplitResponseDto> Splits);
