namespace SplitwiseClone.Api.DTOs;

public record CreateGroupDto(string Name);

public record UpdateGroupDto(string Name);

public record AddGroupMemberDto(Guid UserId);

public record GroupMemberResponseDto(Guid UserId, string? UserName, string? UserEmail, DateTime JoinedAt);

public record GroupResponseDto(int Id, string Name, DateTime CreatedAt, List<GroupMemberResponseDto> Members);
