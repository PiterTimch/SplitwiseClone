namespace SplitwiseClone.Application.Interfaces;

using SplitwiseClone.Application.DTOs.Groups;

public interface IBalanceService
{
    Task<GroupBalancesDto> GetGroupBalancesAsync(int groupId, int currentUserId, CancellationToken cancellationToken = default);
}
