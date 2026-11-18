namespace SplitwiseClone.Application.DTOs.Groups;

public record CreateGroupDto(string Name);

public record UpdateGroupDto(string Name);

public record AddGroupMemberDto(string Email);

public record GroupMemberResponseDto(int UserId, string? UserName, string? UserEmail, DateTime JoinedAt);

public record GroupResponseDto(int Id, string Name, DateTime CreatedAt, List<GroupMemberResponseDto> Members);

public record MemberBalanceDto(int UserId, string UserName, decimal NetBalance);

public record SuggestedSettlementDto(int FromUserId, int ToUserId, decimal Amount);

public record GroupBalancesDto(int GroupId, List<MemberBalanceDto> Members, List<SuggestedSettlementDto> Settlements);
