namespace SplitwiseClone.Application.Services;

using SplitwiseClone.Application.DTOs.Expenses;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Application.Interfaces.Repositories;

public class ExpenseService : IExpenseService
{
    private readonly IUnitOfWork _unitOfWork;

    public ExpenseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ExpenseResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync(cancellationToken);
        var result = new List<ExpenseResponseDto>();

        foreach (var expense in expenses)
        {
            var detailedExpense = await _unitOfWork.Expenses.GetByIdWithDetailsAsync(expense.Id, cancellationToken);
            if (detailedExpense is not null)
            {
                result.Add(MapToResponseDto(detailedExpense));
            }
        }

        return result;
    }

    public async Task<ExpenseResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByIdWithDetailsAsync(id, cancellationToken);
        return expense is null ? null : MapToResponseDto(expense);
    }

    public async Task<IEnumerable<ExpenseResponseDto>> GetByGroupAsync(int groupId, CancellationToken cancellationToken = default)
    {
        var expenses = await _unitOfWork.Expenses.GetByGroupIdAsync(groupId, cancellationToken);
        return expenses.Select(MapToResponseDto).ToList();
    }

    public async Task<ExpenseResponseDto> CreateAsync(Guid currentUserId, CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var creator = await _unitOfWork.Users.GetByIdAsync(currentUserId, cancellationToken);
        if (creator is null)
        {
            throw new BadRequestException("Authenticated user does not have a domain profile.");
        }

        if (dto.GroupId.HasValue)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(dto.GroupId.Value, cancellationToken);
            if (group is null)
            {
                throw new NotFoundException("Group", dto.GroupId.Value);
            }
        }

        var expense = new Expense
        {
            Description = dto.Description,
            TotalAmount = dto.TotalAmount,
            Date = dto.Date ?? DateTime.UtcNow,
            GroupId = dto.GroupId,
            CreatedByUserId = currentUserId,
            IsPayment = dto.IsPayment,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Splits is not null && dto.Splits.Count > 0)
        {
            foreach (var splitDto in dto.Splits)
            {
                expense.Splits.Add(new ExpenseSplit
                {
                    UserId = splitDto.UserId,
                    PaidAmount = splitDto.PaidAmount,
                    OwedAmount = splitDto.OwedAmount
                });
            }
        }

        await _unitOfWork.Expenses.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var detailedExpense = await _unitOfWork.Expenses.GetByIdWithDetailsAsync(expense.Id, cancellationToken);
        return MapToResponseDto(detailedExpense ?? expense);
    }

    public async Task UpdateAsync(int id, UpdateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken);
        if (expense is null)
        {
            throw new NotFoundException("Expense", id);
        }

        expense.Description = dto.Description;
        expense.TotalAmount = dto.TotalAmount;
        expense.Date = dto.Date;
        expense.IsPayment = dto.IsPayment;

        _unitOfWork.Expenses.Update(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken);
        if (expense is null)
        {
            throw new NotFoundException("Expense", id);
        }

        _unitOfWork.Expenses.Delete(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ExpenseResponseDto MapToResponseDto(Expense expense)
    {
        var splitDtos = expense.Splits.Select(s => new ExpenseSplitResponseDto(
            s.Id,
            s.UserId,
            s.User?.Name,
            s.PaidAmount,
            s.OwedAmount)).ToList();

        return new ExpenseResponseDto(
            expense.Id,
            expense.Description,
            expense.TotalAmount,
            expense.Date,
            expense.GroupId,
            expense.Group?.Name,
            expense.CreatedByUserId,
            expense.CreatedByUser?.Name,
            expense.IsPayment,
            expense.CreatedAt,
            splitDtos);
    }
}
