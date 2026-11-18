namespace SplitwiseClone.Application.Services;

using SplitwiseClone.Application.DTOs.Groups;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Domain.Entities;

public sealed class BalanceService(IUnitOfWork unitOfWork) : IBalanceService
{
    public async Task<GroupBalancesDto> GetGroupBalancesAsync(int groupId, int currentUserId, CancellationToken cancellationToken = default)
    {
        var group = await unitOfWork.Groups.GetByIdWithDetailsAsync(groupId, cancellationToken)
            ?? throw new EntityNotFoundException("Group", groupId);
        if (group.Members.All(member => member.UserId != currentUserId))
        {
            throw new GroupAccessDeniedException(currentUserId, groupId);
        }

        var expenses = await unitOfWork.Expenses.GetByGroupIdAsync(groupId, cancellationToken);
        var members = group.Members.ToDictionary(
            member => member.UserId,
            member => new MemberBalanceDto(
                member.UserId,
                member.User?.FullName ?? string.Empty,
                0m));

        foreach (var split in expenses.SelectMany(expense => expense.Splits))
        {
            if (!members.TryGetValue(split.UserId, out var balance))
            {
                throw new EntityConflictException("Expense splits must reference current group members.");
            }

            members[split.UserId] = balance with { NetBalance = balance.NetBalance + split.PaidShare - split.OwedShare };
        }

        var balances = members.Values.OrderBy(member => member.UserId).ToList();
        if (balances.Sum(member => member.NetBalance) != 0m)
        {
            throw new EntityConflictException("Group balances do not sum to zero.");
        }

        return new GroupBalancesDto(groupId, balances, Simplify(balances));
    }

    private static List<SuggestedSettlementDto> Simplify(IEnumerable<MemberBalanceDto> balances)
    {
        var debtors = balances.Where(balance => balance.NetBalance < 0m)
            .Select(balance => new MutableBalance(balance.UserId, -balance.NetBalance))
            .OrderByDescending(balance => balance.Amount)
            .ThenBy(balance => balance.UserId)
            .ToList();
        var creditors = balances.Where(balance => balance.NetBalance > 0m)
            .Select(balance => new MutableBalance(balance.UserId, balance.NetBalance))
            .OrderByDescending(balance => balance.Amount)
            .ThenBy(balance => balance.UserId)
            .ToList();
        var settlements = new List<SuggestedSettlementDto>();
        var debtorIndex = 0;
        var creditorIndex = 0;

        while (debtorIndex < debtors.Count && creditorIndex < creditors.Count)
        {
            var debtor = debtors[debtorIndex];
            var creditor = creditors[creditorIndex];
            var amount = Math.Min(debtor.Amount, creditor.Amount);
            settlements.Add(new SuggestedSettlementDto(debtor.UserId, creditor.UserId, amount));
            debtor.Amount -= amount;
            creditor.Amount -= amount;
            if (debtor.Amount == 0m) debtorIndex++;
            if (creditor.Amount == 0m) creditorIndex++;
        }

        return settlements;
    }

    private sealed class MutableBalance(int userId, decimal amount)
    {
        public int UserId { get; } = userId;
        public decimal Amount { get; set; } = amount;
    }
}
