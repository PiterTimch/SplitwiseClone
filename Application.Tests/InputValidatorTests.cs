using SplitwiseClone.Api.Validations;
using SplitwiseClone.Application.DTOs.Auth;
using SplitwiseClone.Application.DTOs.Expenses;
using SplitwiseClone.Application.DTOs.Groups;
using Xunit;

namespace SplitwiseClone.Application.Tests;

public sealed class InputValidatorTests
{
    [Theory]
    [InlineData("Abcde1A")]
    [InlineData("abcdefgh")]
    [InlineData("Abcdefgh")]
    [InlineData("ABCDEFG1")]
    public void RegisterValidator_RejectsPasswordsThatDoNotMeetIdentityPolicy(string password)
    {
        var validator = new RegisterRequestDtoValidator();
        var result = validator.Validate(new RegisterRequestDto("Alex", "Smith", "alex@example.com", password));

        Assert.Contains(result.Errors, error => error.PropertyName == "Password");
    }

    [Fact]
    public void RegisterValidator_AcceptsPasswordThatMeetsIdentityPolicy()
    {
        var validator = new RegisterRequestDtoValidator();
        var result = validator.Validate(new RegisterRequestDto("Alex", "Smith", "alex@example.com", "Abcdefg1"));

        Assert.DoesNotContain(result.Errors, error => error.PropertyName == "Password");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("Admin")]
    [InlineData("SYSTEM")]
    public void CreateGroupValidator_RejectsShortOrReservedNames(string name)
    {
        var result = new CreateGroupDtoValidator().Validate(new CreateGroupDto(name));

        Assert.Contains(result.Errors, error => error.PropertyName == "Name");
    }

    [Fact]
    public void CreateExpenseValidator_AllowsExpenseWithoutGroup()
    {
        var dto = new CreateExpenseDto("Shared dinner", 10m, null, null, false, ExpenseSplitMode.Equal, [new(1, 10m, 0m)]);

        var result = new CreateExpenseDtoValidator().Validate(dto);

        Assert.DoesNotContain(result.Errors, error => error.PropertyName == "GroupId");
    }

    [Fact]
    public void CreateExpenseValidator_RejectsNonPositiveGroupId()
    {
        var dto = new CreateExpenseDto("Dinner", 10m, null, 0, false, ExpenseSplitMode.Equal, [new(1, 10m, 0m)]);

        var result = new CreateExpenseDtoValidator().Validate(dto);

        Assert.Contains(result.Errors, error => error.PropertyName == "GroupId");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void AddGroupMemberValidator_RejectsInvalidEmail(string email)
    {
        var result = new AddGroupMemberDtoValidator().Validate(new AddGroupMemberDto(email));

        Assert.Contains(result.Errors, error => error.PropertyName == "Email");
    }
}
