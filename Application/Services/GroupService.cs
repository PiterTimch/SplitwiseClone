namespace SplitwiseClone.Application.Services;

using SplitwiseClone.Application.DTOs.Groups;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Application.Interfaces.Repositories;
using SplitwiseClone.Domain.Entities;

public sealed class GroupService(IUnitOfWork unitOfWork) : IGroupService
{
    public async Task<IEnumerable<GroupResponseDto>> GetAllAsync(int currentUserId, CancellationToken cancellationToken = default)
    {
        var memberships = await unitOfWork.GroupMembers.GetByUserIdAsync(currentUserId, cancellationToken);
        var groups = new List<Group?>();
        foreach (var membership in memberships)
        {
            groups.Add(await unitOfWork.Groups.GetByIdWithDetailsAsync(membership.GroupId, cancellationToken));
        }
        return groups.Where(group => group is not null).Select(group => Map(group!)).ToList();
    }

    public async Task<GroupResponseDto> GetByIdAsync(int id, int currentUserId, CancellationToken cancellationToken = default) =>
        Map(await GetGroupAsync(id, currentUserId, cancellationToken));

    public async Task<GroupResponseDto> CreateAsync(int currentUserId, CreateGroupDto dto, CancellationToken cancellationToken = default)
    {
        if (await unitOfWork.Users.GetByIdAsync(currentUserId, cancellationToken) is null)
        {
            throw new EntityNotFoundException("User", currentUserId);
        }

        var group = new Group { Name = dto.Name, CreatedAt = DateTime.UtcNow };
        group.Members.Add(new GroupMember { UserId = currentUserId, JoinedAt = DateTime.UtcNow });
        await unitOfWork.Groups.AddAsync(group, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(await unitOfWork.Groups.GetByIdWithDetailsAsync(group.Id, cancellationToken) ?? group);
    }

    public async Task UpdateAsync(int id, int currentUserId, UpdateGroupDto dto, CancellationToken cancellationToken = default)
    {
        var group = await GetGroupAsync(id, currentUserId, cancellationToken);
        group.Name = dto.Name;
        unitOfWork.Groups.Update(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, int currentUserId, CancellationToken cancellationToken = default)
    {
        var group = await GetGroupAsync(id, currentUserId, cancellationToken);
        if ((await unitOfWork.Expenses.GetByGroupIdAsync(id, cancellationToken)).Any())
        {
            throw new EntityConflictException("A group with expense history cannot be deleted.");
        }

        unitOfWork.Groups.Delete(group);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<GroupMemberResponseDto> AddMemberAsync(int groupId, int currentUserId, AddGroupMemberDto dto, CancellationToken cancellationToken = default)
    {
        await GetGroupAsync(groupId, currentUserId, cancellationToken);
        var email = dto.Email.Trim();
        var user = await unitOfWork.Users.GetByEmailAsync(email, cancellationToken)
            ?? throw new EntityNotFoundException("User", email);
        if (await unitOfWork.GroupMembers.GetAsync(groupId, user.Id, cancellationToken) is not null)
        {
            throw new EntityConflictException("User is already a member of this group.");
        }

        var member = new GroupMember { GroupId = groupId, UserId = user.Id, User = user, JoinedAt = DateTime.UtcNow };
        await unitOfWork.GroupMembers.AddAsync(member, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Map(member);
    }

    public async Task RemoveMemberAsync(int groupId, int userId, int currentUserId, CancellationToken cancellationToken = default)
    {
        await GetGroupAsync(groupId, currentUserId, cancellationToken);
        var member = await unitOfWork.GroupMembers.GetAsync(groupId, userId, cancellationToken)
            ?? throw new EntityNotFoundException("GroupMember", $"{groupId}/{userId}");
        var expenses = await unitOfWork.Expenses.GetByGroupIdAsync(groupId, cancellationToken);
        if (expenses.Any(expense => expense.CreatedByUserId == userId || expense.Splits.Any(split => split.UserId == userId)))
        {
            throw new EntityConflictException("A group member with expense history cannot be removed.");
        }

        unitOfWork.GroupMembers.Delete(member);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Group> GetGroupAsync(int groupId, int currentUserId, CancellationToken cancellationToken)
    {
        var group = await unitOfWork.Groups.GetByIdWithDetailsAsync(groupId, cancellationToken)
            ?? throw new EntityNotFoundException("Group", groupId);
        if (group.Members.All(member => member.UserId != currentUserId))
        {
            throw new GroupAccessDeniedException(currentUserId, groupId);
        }

        return group;
    }

    private static GroupResponseDto Map(Group group) => new(
        group.Id,
        group.Name,
        group.CreatedAt,
        group.Members.Select(Map).ToList());

    private static GroupMemberResponseDto Map(GroupMember member) => new(
        member.UserId,
        member.User?.FullName,
        member.User?.Email,
        member.JoinedAt);
}
