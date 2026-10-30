namespace SplitwiseClone.Api.Validations;

using FluentValidation;
using SplitwiseClone.Application.DTOs.Expenses;

public class CreateExpenseSplitDtoValidator : AbstractValidator<CreateExpenseSplitDto>
{
    public CreateExpenseSplitDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PaidAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.OwedAmount).GreaterThanOrEqualTo(0);
    }
}

public class CreateExpenseDtoValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TotalAmount).GreaterThan(0);
        RuleForEach(x => x.Splits).SetValidator(new CreateExpenseSplitDtoValidator());
    }
}

public class UpdateExpenseDtoValidator : AbstractValidator<UpdateExpenseDto>
{
    public UpdateExpenseDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TotalAmount).GreaterThan(0);
    }
}
