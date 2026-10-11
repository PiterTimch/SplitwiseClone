namespace SplitwiseClone.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using SplitwiseClone.Api.DTOs;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Infrastructure.Repositories.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class GroupsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public GroupsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetAll(CancellationToken cancellationToken)
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

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GroupResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var group = await _unitOfWork.Groups.GetByIdWithDetailsAsync(id, cancellationToken);
        if (group is null)
        {
            return NotFound();
        }

        var memberDtos = group.Members.Select(m => new GroupMemberResponseDto(
            m.UserId,
            m.User?.Name,
            m.User?.Email,
            m.JoinedAt)).ToList();

        return Ok(new GroupResponseDto(group.Id, group.Name, group.CreatedAt, memberDtos));
    }

    [HttpPost]
    public async Task<ActionResult<GroupResponseDto>> Create([FromBody] CreateGroupDto dto, CancellationToken cancellationToken)
    {
        var group = new Group
        {
            Name = dto.Name,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Groups.AddAsync(group, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var responseDto = new GroupResponseDto(group.Id, group.Name, group.CreatedAt, new List<GroupMemberResponseDto>());
        return CreatedAtAction(nameof(GetById), new { id = group.Id }, responseDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGroupDto dto, CancellationToken cancellationToken)
    {
        var group = await _unitOfWork.Groups.GetByIdAsync(id, cancellationToken);
        if (group is null)
        {
            return NotFound();
        }

        group.Name = dto.Name;

        _unitOfWork.Groups.Update(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var group = await _unitOfWork.Groups.GetByIdAsync(id, cancellationToken);
        if (group is null)
        {
            return NotFound();
        }

        _unitOfWork.Groups.Delete(group);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:int}/members")]
    public async Task<IActionResult> AddMember(int id, [FromBody] AddGroupMemberDto dto, CancellationToken cancellationToken)
    {
        var group = await _unitOfWork.Groups.GetByIdAsync(id, cancellationToken);
        if (group is null)
        {
            return NotFound(new { message = "Group not found." });
        }

        var user = await _unitOfWork.Users.GetByIdAsync(dto.UserId, cancellationToken);
        if (user is null)
        {
            return NotFound(new { message = "User not found." });
        }

        var existingMember = await _unitOfWork.GroupMembers.GetAsync(id, dto.UserId, cancellationToken);
        if (existingMember is not null)
        {
            return Conflict(new { message = "User is already a member of this group." });
        }

        var member = new GroupMember
        {
            GroupId = id,
            UserId = dto.UserId,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.GroupMembers.AddAsync(member, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Ok(new GroupMemberResponseDto(member.UserId, user.Name, user.Email, member.JoinedAt));
    }

    [HttpDelete("{id:int}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(int id, Guid userId, CancellationToken cancellationToken)
    {
        var member = await _unitOfWork.GroupMembers.GetAsync(id, userId, cancellationToken);
        if (member is null)
        {
            return NotFound();
        }

        _unitOfWork.GroupMembers.Delete(member);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
