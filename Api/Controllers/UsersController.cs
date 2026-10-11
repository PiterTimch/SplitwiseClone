namespace SplitwiseClone.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using SplitwiseClone.Api.DTOs;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Infrastructure.Repositories.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public UsersController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.Users.GetAllAsync(cancellationToken);
        var result = users.Select(u => new UserResponseDto(u.Id, u.Name, u.Email, u.CreatedAt));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(new UserResponseDto(user.Id, user.Name, user.Email, user.CreatedAt));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(dto.Email, cancellationToken);
        if (existingUser is not null)
        {
            return Conflict(new { message = "User with this email already exists." });
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var responseDto = new UserResponseDto(user.Id, user.Name, user.Email, user.CreatedAt);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, responseDto);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        user.Name = dto.Name;
        user.Email = dto.Email;

        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        _unitOfWork.Users.Delete(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
