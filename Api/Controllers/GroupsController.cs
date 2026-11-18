namespace SplitwiseClone.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitwiseClone.Application.DTOs.Groups;
using SplitwiseClone.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GroupsController(IGroupService groupService, IBalanceService balanceService) : AuthenticatedControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GroupResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await groupService.GetAllAsync(CurrentUserId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<GroupResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await groupService.GetByIdAsync(id, CurrentUserId, cancellationToken));
    }

    [HttpGet("{id:int}/balances")]
    public async Task<ActionResult<GroupBalancesDto>> GetBalances(int id, CancellationToken cancellationToken)
    {
        return Ok(await balanceService.GetGroupBalancesAsync(id, CurrentUserId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<GroupResponseDto>> Create([FromBody] CreateGroupDto dto, CancellationToken cancellationToken)
    {
        var result = await groupService.CreateAsync(CurrentUserId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateGroupDto dto, CancellationToken cancellationToken)
    {
        await groupService.UpdateAsync(id, CurrentUserId, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await groupService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:int}/members")]
    public async Task<ActionResult<GroupMemberResponseDto>> AddMember(
        int id,
        [FromBody] AddGroupMemberDto dto,
        CancellationToken cancellationToken)
    {
        var result = await groupService.AddMemberAsync(id, CurrentUserId, dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}/members/{userId:int}")]
    public async Task<IActionResult> RemoveMember(int id, int userId, CancellationToken cancellationToken)
    {
        await groupService.RemoveMemberAsync(id, userId, CurrentUserId, cancellationToken);
        return NoContent();
    }
}
