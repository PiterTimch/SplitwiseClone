namespace SplitwiseClone.Api.Validations;

using FluentValidation;
using SplitwiseClone.Application.DTOs.Auth;

public class RegisterRequestDtoValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestDtoValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100)
            .WithMessage("First name is required and cannot exceed 100 characters.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100)
            .WithMessage("Last name is required and cannot exceed 100 characters.");
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop).NotEmpty().EmailAddress()
            .WithMessage("A valid email address is required.");
        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.");
    }
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop).NotEmpty().EmailAddress()
            .WithMessage("A valid email address is required.");
        RuleFor(x => x.Password).NotEmpty()
            .WithMessage("Password is required.");
    }
}
