using SplitwiseClone.Api.Validations;
using SplitwiseClone.Application.DTOs.Auth;
using SplitwiseClone.Application.DTOs.Expenses;
using SplitwiseClone.Application.DTOs.Groups;
using SplitwiseClone.Application.DTOs.Users;
using Xunit;

namespace SplitwiseClone.Application.Tests;

public sealed class RequestValidatorsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void LoginRequest_InvalidEmail_ReturnsOneError(string email)
    {
        var result = new LoginRequestDtoValidator().Validate(new LoginRequestDto(email, "secret"));

        Assert.Single(result.Errors, error => error.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("Ad")]
    [InlineData("Admin")]
    public void CreateGroup_InvalidName_ReturnsOneError(string name)
    {
        var result = new CreateGroupDtoValidator().Validate(new CreateGroupDto(name));

        Assert.Single(result.Errors, error => error.PropertyName == "Name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Root")]
    public void UpdateGroup_InvalidName_ReturnsOneError(string name)
    {
        var result = new UpdateGroupDtoValidator().Validate(new UpdateGroupDto(name));

        Assert.Single(result.Errors, error => error.PropertyName == "Name");
    }

    [Theory]
    [InlineData("")]
    [InlineData("invalid-email")]
    public void AddGroupMember_InvalidEmail_ReturnsOneError(string email)
    {
        var result = new AddGroupMemberDtoValidator().Validate(new AddGroupMemberDto(email));

        Assert.Single(result.Errors, error => error.PropertyName == "Email");
    }

    [Fact]
    public void UpdateUser_EmptyEmail_ReturnsOneError()
    {
        var result = new UpdateUserDtoValidator().Validate(new UpdateUserDto("Petro", "Tymchuk", ""));

        Assert.Single(result.Errors, error => error.PropertyName == "Email");
    }

    [Fact]
    public void CreateExpense_NegativeTotalWithExtraDecimals_ReturnsOneError()
    {
        var dto = new CreateExpenseDto("Dinner", -1.234m, null, null, false, ExpenseSplitMode.Equal,
            [new CreateExpenseSplitDto(1, 1m, 1m)]);

        var result = new CreateExpenseDtoValidator().Validate(dto);

        Assert.Single(result.Errors, error => error.PropertyName == "TotalCost");
    }

    [Fact]
    public void UpdateExpense_NegativeTotalWithExtraDecimals_ReturnsOneError()
    {
        var dto = new UpdateExpenseDto("Dinner", -1.234m, DateTime.UtcNow, false, ExpenseSplitMode.Equal,
            [new CreateExpenseSplitDto(1, 1m, 1m)]);

        var result = new UpdateExpenseDtoValidator().Validate(dto);

        Assert.Single(result.Errors, error => error.PropertyName == "TotalCost");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExpenseWithNullSplits_ReturnsOneError(bool update)
    {
        var errors = update
            ? new UpdateExpenseDtoValidator().Validate(new UpdateExpenseDto("Dinner", 10m, DateTime.UtcNow, false, ExpenseSplitMode.Equal, null!)).Errors
            : new CreateExpenseDtoValidator().Validate(new CreateExpenseDto("Dinner", 10m, null, null, false, ExpenseSplitMode.Equal, null)).Errors;

        Assert.Single(errors, error => error.PropertyName == "Splits");
    }

    [Fact]
    public void ExpenseSplit_NegativePaidShareWithExtraDecimals_ReturnsOneError()
    {
        var result = new CreateExpenseSplitDtoValidator().Validate(new CreateExpenseSplitDto(1, -1.234m, 1m));

        Assert.Single(result.Errors, error => error.PropertyName == "PaidShare");
    }
}
