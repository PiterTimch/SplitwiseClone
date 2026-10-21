namespace SplitwiseClone.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitwiseClone.Application.DTOs.Groups;
using SplitwiseClone.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController : ControllerBase
{
    private readonly IGroupService _groupService;

    public GroupsController(IGroupService groupService)
    {
        _groupService = groupService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _groupService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GroupResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _groupService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<GroupResponseDto>> Create([FromBody] CreateGroupDto dto, CancellationToken cancellationToken)
    {
        var result = await _groupService.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGroupDto dto, CancellationToken cancellationToken)
    {
        await _groupService.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _groupService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/members")]
    public async Task<ActionResult<GroupMemberResponseDto>> AddMember(
        int id,
        [FromBody] AddGroupMemberDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _groupService.AddMemberAsync(id, dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}/members/{userId:guid}")]
    public async Task<IActionResult> RemoveMember(int id, Guid userId, CancellationToken cancellationToken)
    {
        await _groupService.RemoveMemberAsync(id, userId, cancellationToken);
        return NoContent();
    }
}
