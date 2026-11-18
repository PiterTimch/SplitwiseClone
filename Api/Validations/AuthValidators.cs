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
        RuleFor(x => x.Email).NotEmpty().EmailAddress()
            .WithMessage("A valid email address is required.");
        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("[0-9]")
            .WithMessage("Password must be at least 8 characters and include an uppercase letter, a lowercase letter, and a digit.");
    }
}

public class LoginRequestDtoValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestDtoValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress()
            .WithMessage("A valid email address is required.");
        RuleFor(x => x.Password).NotEmpty()
            .WithMessage("Password is required.");
    }
}
