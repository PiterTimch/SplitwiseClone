namespace SplitwiseClone.Api.Validations;

using FluentValidation;
using SplitwiseClone.Application.DTOs.Expenses;

public class CreateExpenseSplitDtoValidator : AbstractValidator<CreateExpenseSplitDto>
{
    public CreateExpenseSplitDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.PaidShare).Cascade(CascadeMode.Stop)
            .GreaterThanOrEqualTo(0)
            .Must(value => decimal.Round(value, 2) == value);
        RuleFor(x => x.SplitValue).GreaterThanOrEqualTo(0);
    }
}

public class CreateExpenseDtoValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TotalCost).Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .Must(value => decimal.Round(value, 2) == value);
        RuleFor(x => x.GroupId).GreaterThan(0).When(x => x.GroupId.HasValue);
        RuleFor(x => x.Splits).Cascade(CascadeMode.Stop).NotNull().NotEmpty();
        RuleFor(x => x.SplitMode).IsInEnum();
        RuleForEach(x => x.Splits).SetValidator(new CreateExpenseSplitDtoValidator());
    }
}

public class UpdateExpenseDtoValidator : AbstractValidator<UpdateExpenseDto>
{
    public UpdateExpenseDtoValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.TotalCost).Cascade(CascadeMode.Stop)
            .GreaterThan(0)
            .Must(value => decimal.Round(value, 2) == value);
        RuleFor(x => x.Splits).Cascade(CascadeMode.Stop).NotNull().NotEmpty();
        RuleFor(x => x.SplitMode).IsInEnum();
        RuleForEach(x => x.Splits).SetValidator(new CreateExpenseSplitDtoValidator());
    }
}
