namespace SplitwiseClone.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitwiseClone.Application.DTOs.Users;
using SplitwiseClone.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class UsersController(IUserService userService) : AuthenticatedControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserResponseDto>> GetCurrentUser(CancellationToken cancellationToken) =>
        Ok(await userService.GetByIdAsync(CurrentUserId, cancellationToken));

    [HttpPut("me")]
    public async Task<ActionResult<UserResponseDto>> UpdateCurrentUser(UpdateUserDto dto, CancellationToken cancellationToken) =>
        Ok(await userService.UpdateAsync(CurrentUserId, dto, cancellationToken));

    [HttpDelete("me")]
    public async Task<IActionResult> DeleteCurrentUser(CancellationToken cancellationToken)
    {
        await userService.DeleteAsync(CurrentUserId, cancellationToken);
        return NoContent();
    }
}
