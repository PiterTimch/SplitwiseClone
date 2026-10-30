namespace SplitwiseClone.Api.Validations;

using FluentValidation;
using SplitwiseClone.Application.DTOs.Groups;

public class CreateGroupDtoValidator : AbstractValidator<CreateGroupDto>
{
    public CreateGroupDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class UpdateGroupDtoValidator : AbstractValidator<UpdateGroupDto>
{
    public UpdateGroupDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public class AddGroupMemberDtoValidator : AbstractValidator<AddGroupMemberDto>
{
    public AddGroupMemberDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
