namespace SplitwiseClone.Api.Validations;

using FluentValidation;
using SplitwiseClone.Application.DTOs.Groups;

public class CreateGroupDtoValidator : AbstractValidator<CreateGroupDto>
{
    private static readonly string[] ReservedNames = ["Admin", "System", "Root"];

    public CreateGroupDtoValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Length(3, 100)
            .Must(name => !ReservedNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Group name must be between 3 and 100 characters and cannot be a reserved name.");
    }
}

public class UpdateGroupDtoValidator : AbstractValidator<UpdateGroupDto>
{
    private static readonly string[] ReservedNames = ["Admin", "System", "Root"];

    public UpdateGroupDtoValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Length(3, 100)
            .Must(name => !ReservedNames.Contains(name, StringComparer.OrdinalIgnoreCase))
            .WithMessage("Group name must be between 3 and 100 characters and cannot be a reserved name.");
    }
}

public class AddGroupMemberDtoValidator : AbstractValidator<AddGroupMemberDto>
{
    public AddGroupMemberDtoValidator()
    {
        RuleFor(x => x.Email).Cascade(CascadeMode.Stop).NotEmpty().EmailAddress()
            .WithMessage("A valid email address is required.");
    }
}
