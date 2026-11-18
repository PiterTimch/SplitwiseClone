namespace SplitwiseClone.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SplitwiseClone.Application.DTOs.Expenses;
using SplitwiseClone.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ExpensesController(IExpenseService expenseService) : AuthenticatedControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        return Ok(await expenseService.GetAllAsync(CurrentUserId, cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        return Ok(await expenseService.GetByIdAsync(id, CurrentUserId, cancellationToken));
    }

    [HttpGet("group/{groupId:int}")]
    public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetByGroup(int groupId, CancellationToken cancellationToken)
    {
        return Ok(await expenseService.GetByGroupAsync(groupId, CurrentUserId, cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponseDto>> Create(
        [FromBody] CreateExpenseDto dto,
        CancellationToken cancellationToken)
    {
        var result = await expenseService.CreateAsync(CurrentUserId, dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto dto, CancellationToken cancellationToken)
    {
        await expenseService.UpdateAsync(id, CurrentUserId, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await expenseService.DeleteAsync(id, CurrentUserId, cancellationToken);
        return NoContent();
    }

}
