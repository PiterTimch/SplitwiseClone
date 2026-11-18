namespace SplitwiseClone.Application.Services;

using SplitwiseClone.Application.DTOs.Expenses;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Domain.Entities;

public sealed class ExpenseService(IUnitOfWork unitOfWork) : IExpenseService
{
    public async Task<IEnumerable<ExpenseResponseDto>> GetAllAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var memberships = await unitOfWork.GroupMembers.GetByUserIdAsync(currentUserId, cancellationToken);
        var expenses = new List<Expense>();
        foreach (var membership in memberships)
        {
            expenses.AddRange(await unitOfWork.Expenses.GetByGroupIdAsync(membership.GroupId, cancellationToken));
        }

        return expenses.DistinctBy(expense => expense.Id).Select(Map).ToList();
    }

    public async Task<ExpenseResponseDto> GetByIdAsync(int id, int currentUserId, CancellationToken cancellationToken = default)
    {
        var expense = await unitOfWork.Expenses.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Expense", id);
        await EnsureMemberAsync(expense.GroupId, currentUserId, cancellationToken);
        return Map(expense);
    }

    public async Task<IEnumerable<ExpenseResponseDto>> GetByGroupAsync(int groupId, int currentUserId, CancellationToken cancellationToken = default)
    {
        await EnsureMemberAsync(groupId, currentUserId, cancellationToken);
        var expenses = await unitOfWork.Expenses.GetByGroupIdAsync(groupId, cancellationToken);
        return expenses.Select(Map).ToList();
    }

    public async Task<ExpenseResponseDto> CreateAsync(int currentUserId, CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var groupId = dto.GroupId;
        var group = await GetGroupAsync(groupId, cancellationToken);
        EnsureMembersInSplit(group, dto.Splits!, currentUserId);
        var splits = ExpenseSplitCalculator.Calculate(dto.TotalCost, dto.SplitMode, dto.Splits!);

        var expense = new Expense
        {
            Description = dto.Description.Trim(),
            TotalCost = dto.TotalCost,
            Date = dto.Date ?? DateTime.UtcNow,
            GroupId = group.Id,
            CreatedByUserId = currentUserId,
            IsPayment = dto.IsPayment,
            CreatedAt = DateTime.UtcNow,
            Splits = splits.ToList()
        };

        await unitOfWork.Expenses.AddAsync(expense, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(await unitOfWork.Expenses.GetByIdWithDetailsAsync(expense.Id, cancellationToken) ?? expense);
    }

    public async Task UpdateAsync(int id, int currentUserId, UpdateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var expense = await unitOfWork.Expenses.GetByIdWithSplitsAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Expense", id);
        await EnsureMemberAsync(expense.GroupId, currentUserId, cancellationToken);
        if (expense.CreatedByUserId != currentUserId)
        {
            throw new GroupAccessDeniedException(currentUserId, expense.GroupId);
        }

        var group = await GetGroupAsync(expense.GroupId, cancellationToken);
        EnsureMembersInSplit(group, dto.Splits, currentUserId);
        var splits = ExpenseSplitCalculator.Calculate(dto.TotalCost, dto.SplitMode, dto.Splits);
        expense.Description = dto.Description.Trim();
        expense.TotalCost = dto.TotalCost;
        expense.Date = dto.Date;
        expense.IsPayment = dto.IsPayment;
        foreach (var oldSplit in expense.Splits.ToList())
        {
            unitOfWork.ExpenseSplits.Delete(oldSplit);
        }

        foreach (var split in splits)
        {
            expense.Splits.Add(split);
        }

        unitOfWork.Expenses.Update(expense);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default)
    {
        var expense = await unitOfWork.Expenses.GetByIdAsync(id, cancellationToken)
            ?? throw new EntityNotFoundException("Expense", id);
        await EnsureMemberAsync(expense.GroupId, currentUserId, cancellationToken);
        if (expense.CreatedByUserId != currentUserId)
        {
            throw new GroupAccessDeniedException(currentUserId, expense.GroupId);
        }

        unitOfWork.Expenses.Delete(expense);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Group> GetGroupAsync(int groupId, CancellationToken cancellationToken) =>
        await unitOfWork.Groups.GetByIdWithDetailsAsync(groupId, cancellationToken)
        ?? throw new EntityNotFoundException("Group", groupId);

    private async Task EnsureMemberAsync(int groupId, int currentUserId, CancellationToken cancellationToken)
    {
        var group = await GetGroupAsync(groupId, cancellationToken);
        if (group.Members.All(member => member.UserId != currentUserId))
        {
            throw new GroupAccessDeniedException(currentUserId, groupId);
        }
    }

    private static void EnsureMembersInSplit(Group group, IReadOnlyCollection<CreateExpenseSplitDto> splits, int currentUserId)
    {
        var memberIds = group.Members.Select(member => member.UserId).ToHashSet();
        if (splits.Any(split => !memberIds.Contains(split.UserId)))
        {
            var invalidUserId = splits.First(split => !memberIds.Contains(split.UserId)).UserId;
            throw new GroupAccessDeniedException(invalidUserId, group.Id);
        }

        if (splits.Select(split => split.UserId).Distinct().Count() != splits.Count)
        {
            throw new EntityConflictException("An expense can contain only one split per group member.");
        }

        if (!memberIds.Contains(currentUserId))
        {
            throw new GroupAccessDeniedException(currentUserId, group.Id);
        }
    }

    private static ExpenseResponseDto Map(Expense expense) => new(
        expense.Id,
        expense.Description,
        expense.TotalCost,
        expense.Date,
        expense.GroupId,
        expense.Group?.Name,
        expense.CreatedByUserId,
        expense.CreatedByUser?.FullName,
        expense.IsPayment,
        expense.CreatedAt,
        expense.Splits.Select(split => new ExpenseSplitResponseDto(
            split.Id,
            split.UserId,
            split.User?.FullName,
            split.PaidShare,
            split.OwedShare)).ToList());
}
