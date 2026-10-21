namespace SplitwiseClone.Application.Interfaces;

using SplitwiseClone.Application.DTOs.Groups;

public interface IGroupService
{
    Task<IEnumerable<GroupResponseDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<GroupResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<GroupResponseDto> CreateAsync(CreateGroupDto dto, CancellationToken cancellationToken = default);
    Task UpdateAsync(int id, UpdateGroupDto dto, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<GroupMemberResponseDto> AddMemberAsync(int groupId, AddGroupMemberDto dto, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(int groupId, Guid userId, CancellationToken cancellationToken = default);
}
