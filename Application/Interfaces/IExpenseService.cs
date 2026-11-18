namespace SplitwiseClone.Application.Interfaces;

using SplitwiseClone.Application.DTOs.Expenses;

public interface IExpenseService
{
    Task<IEnumerable<ExpenseResponseDto>> GetAllAsync(int currentUserId, CancellationToken cancellationToken = default);
    Task<ExpenseResponseDto> GetByIdAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExpenseResponseDto>> GetByGroupAsync(int groupId, int currentUserId, CancellationToken cancellationToken = default);
    Task<ExpenseResponseDto> CreateAsync(int currentUserId, CreateExpenseDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, int currentUserId, UpdateExpenseDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
}
