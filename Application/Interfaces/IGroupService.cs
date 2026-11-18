namespace SplitwiseClone.Application.Interfaces;

using SplitwiseClone.Application.DTOs.Groups;

public interface IGroupService
{
    Task<IEnumerable<GroupResponseDto>> GetAllAsync(int currentUserId, CancellationToken cancellationToken = default);
    Task<GroupResponseDto> GetByIdAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<GroupResponseDto> CreateAsync(int currentUserId, CreateGroupDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, int currentUserId, UpdateGroupDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default);
    Task<GroupMemberResponseDto> AddMemberAsync(int groupId, int currentUserId, AddGroupMemberDto dto, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(int groupId, int userId, int currentUserId, CancellationToken cancellationToken = default);
}
