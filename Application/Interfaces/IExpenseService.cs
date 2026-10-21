namespace SplitwiseClone.Application.Interfaces;

using SplitwiseClone.Application.DTOs.Expenses;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<ExpenseResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExpenseResponseDto>> GetByGroupAsync(int groupId, CancellationToken cancellationToken = default);
    Task<ExpenseResponseDto> CreateAsync(Guid currentUserId, CreateExpenseDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateExpenseDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
