namespace SplitwiseClone.Application.DTOs.Expenses;

public record CreateExpenseSplitDto(Guid UserId, double PaidAmount, double OwedAmount);

public record CreateExpenseDto(
    string Description,
    double TotalAmount,
    DateTime? Date,
    int? GroupId,
    bool IsPayment,
    List<CreateExpenseSplitDto>? Splits);

public record UpdateExpenseDto(
    string Description,
    double TotalAmount,
    DateTime Date,
    bool IsPayment);

public record ExpenseSplitResponseDto(int Id, Guid UserId, string? UserName, double PaidAmount, double OwedAmount);

public record ExpenseResponseDto(
    int Id,
    string Description,
    double TotalAmount,
    DateTime Date,
    int? GroupId,
    string? GroupName,
    Guid CreatedByUserId,
    string? CreatedByUserName,
    bool IsPayment,
    DateTime CreatedAt,
    List<ExpenseSplitResponseDto> Splits);
