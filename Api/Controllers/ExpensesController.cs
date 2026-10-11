namespace SplitwiseClone.Api.Controllers;

using Microsoft.AspNetCore.Mvc;
using SplitwiseClone.Api.DTOs;
using SplitwiseClone.Domain.Entities;
using SplitwiseClone.Infrastructure.Repositories.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ExpensesController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var expenses = await _unitOfWork.Expenses.GetAllAsync(cancellationToken);
        var result = new List<ExpenseResponseDto>();

        foreach (var expense in expenses)
        {
            var detailedExpense = await _unitOfWork.Expenses.GetByIdWithDetailsAsync(expense.Id, cancellationToken);
            if (detailedExpense is not null)
            {
                result.Add(MapToResponseDto(detailedExpense));
            }
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ExpenseResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var expense = await _unitOfWork.Expenses.GetByIdWithDetailsAsync(id, cancellationToken);
        if (expense is null)
        {
            return NotFound();
        }

        return Ok(MapToResponseDto(expense));
    }

    [HttpGet("group/{groupId:int}")]
    public async Task<ActionResult<IEnumerable<ExpenseResponseDto>>> GetByGroup(int groupId, CancellationToken cancellationToken)
    {
        var expenses = await _unitOfWork.Expenses.GetByGroupIdAsync(groupId, cancellationToken);
        var result = expenses.Select(MapToResponseDto).ToList();
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponseDto>> Create([FromBody] CreateExpenseDto dto, CancellationToken cancellationToken)
    {
        var creator = await _unitOfWork.Users.GetByIdAsync(dto.CreatedByUserId, cancellationToken);
        if (creator is null)
        {
            return BadRequest(new { message = "Creator user does not exist." });
        }

        if (dto.GroupId.HasValue)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(dto.GroupId.Value, cancellationToken);
            if (group is null)
            {
                return BadRequest(new { message = "Group does not exist." });
            }
        }

        var expense = new Expense
        {
            Description = dto.Description,
            TotalAmount = dto.TotalAmount,
            Date = dto.Date ?? DateTime.UtcNow,
            GroupId = dto.GroupId,
            CreatedByUserId = dto.CreatedByUserId,
            IsPayment = dto.IsPayment,
            CreatedAt = DateTime.UtcNow
        };

        if (dto.Splits is not null && dto.Splits.Count > 0)
        {
            foreach (var splitDto in dto.Splits)
            {
                expense.Splits.Add(new ExpenseSplit
                {
                    UserId = splitDto.UserId,
                    PaidAmount = splitDto.PaidAmount,
                    OwedAmount = splitDto.OwedAmount
                });
            }
        }

        await _unitOfWork.Expenses.AddAsync(expense, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var detailedExpense = await _unitOfWork.Expenses.GetByIdWithDetailsAsync(expense.Id, cancellationToken);
        var responseDto = MapToResponseDto(detailedExpense ?? expense);

        return CreatedAtAction(nameof(GetById), new { id = expense.Id }, responseDto);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto dto, CancellationToken cancellationToken)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken);
        if (expense is null)
        {
            return NotFound();
        }

        expense.Description = dto.Description;
        expense.TotalAmount = dto.TotalAmount;
        expense.Date = dto.Date;
        expense.IsPayment = dto.IsPayment;

        _unitOfWork.Expenses.Update(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var expense = await _unitOfWork.Expenses.GetByIdAsync(id, cancellationToken);
        if (expense is null)
        {
            return NotFound();
        }

        _unitOfWork.Expenses.Delete(expense);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private static ExpenseResponseDto MapToResponseDto(Expense expense)
    {
        var splitDtos = expense.Splits.Select(s => new ExpenseSplitResponseDto(
            s.Id,
            s.UserId,
            s.User?.Name,
            s.PaidAmount,
            s.OwedAmount)).ToList();

        return new ExpenseResponseDto(
            expense.Id,
            expense.Description,
            expense.TotalAmount,
            expense.Date,
            expense.GroupId,
            expense.Group?.Name,
            expense.CreatedByUserId,
            expense.CreatedByUser?.Name,
            expense.IsPayment,
            expense.CreatedAt,
            splitDtos);
    }
}
