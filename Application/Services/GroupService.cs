namespace SplitwiseClone.Application.Services;

using SplitwiseClone.Application.DTOs.Groups;
using SplitwiseClone.Application.Exceptions;
using SplitwiseClone.Application.Interfaces;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Application.Interfaces.Repositories;

public class GroupService : IGroupService
{
    private readonly IUnitOfWork _unitOfWork;

    public GroupService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<GroupResponseDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var groups = await _unitOfWork.Groups.GetAllAsync(cancellationToken);
        var result = new List<GroupResponseDto>();

        foreach (var group in groups)
        {
            var members = await _unitOfWork.GroupMembers.GetByGroupIdAsync(group.Id, cancellationToken);
            var memberDtos = members.Select(m => new GroupMemberResponseDto(
                m.UserId,
                m.User?.Name,
                m.User?.Email,
                m.JoinedAt)).ToList();

            result.Add(new GroupResponseDto(group.Id, group.Name, group.CreatedAt, memberDtos));
        }

        return result;
    }

    public async Task<GroupResponseDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var group = await _unitOfWork.Groups.GetByIdWithDetailsAsync(id, cancellationToken);
        if (group is null)
        {
            return null;
        }

        var memberDtos = group.Members.Select(m => new GroupMemberResponseDto(
            m.UserId,
            m.User?.Name,
            m.User?.Email,
            m.JoinedAt)).ToList();

        return new GroupResponseDto(group.Id, group.Name, group.CreatedAt, memberDtos);
    }

    public async Task<GroupResponseDto> CreateAsync(CreateGroupDto dto, CancellationToken cancellationToken = default)
    {
        var group = new Group
        {
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Groups.AddAsync(group, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new GroupResponseDto(group.Id, group.Name, group.CreatedAt, new List<GroupMemberResponseDto>());
    }

    public async Task UpdateAsync(int id, UpdateGroupDto dto, CancellationToken cancellationToken = default)
    {
        var group = await _unitOfWork.Groups.GetByIdAsync(id, cancellationToken);
        if (group is null)
        {
            throw new NotFoundException("Group", id);
        }

        group.Name = dto.Name;
        _unitOfWork.Groups.Update(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var group = await _unitOfWork.Groups.GetByIdAsync(id, cancellationToken);
        if (group is null)
        {
            throw new NotFoundException("Group", id);
        }

        _unitOfWork.Groups.Delete(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<GroupMemberResponseDto> AddMemberAsync(int groupId, AddGroupMemberDto dto, CancellationToken cancellationToken = default)
    {
        var group = await _unitOfWork.Groups.GetByIdAsync(groupId, cancellationToken);
        if (group is null)
        {
            throw new NotFoundException("Group", groupId);
        }

        var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User", dto.UserId);
        }

        var existingMember = await _unitOfWork.GroupMembers.GetAsync(groupId, dto.UserId, cancellationToken);
        if (existingMember is not null)
        {
            throw new ConflictException("User is already a member of this group.");
        }

        var member = new GroupMember
        {
            GroupId = groupId,
            UserId = dto.UserId,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.GroupMembers.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new GroupMemberResponseDto(member.UserId, user.Name, user.Email, member.JoinedAt);
    }

    public async Task RemoveMemberAsync(int groupId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _unitOfWork.GroupMembers.GetAsync(groupId, userId, cancellationToken);
        if (member is null)
        {
            throw new NotFoundException("GroupMember", $"{groupId}/{userId}");
        }

        _unitOfWork.GroupMembers.Delete(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
