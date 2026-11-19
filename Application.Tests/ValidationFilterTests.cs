using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using SplitwiseClone.Api.Filters;
using SplitwiseClone.Api.Validations;
using SplitwiseClone.Application.DTOs.Auth;
using Xunit;

namespace SplitwiseClone.Application.Tests;

public sealed class ValidationFilterTests
{
    [Fact]
    public async Task InvalidDto_ReturnsProblemDetailsWithoutThrowing()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<RegisterRequestDto>, RegisterRequestDtoValidator>();
        using var provider = services.BuildServiceProvider();
        var context = CreateContext(provider, new RegisterRequestDto("Petro", "Tymchuk", "petro@example.com", "123456"));
        var nextCalled = false;
        var filter = new ValidationFilter(provider);

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, [], new object()));
        });

        Assert.False(nextCalled);
        var result = Assert.IsType<ObjectResult>(context.Result);
        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.Contains("application/problem+json", result.ContentTypes);
        var details = Assert.IsType<HttpValidationProblemDetails>(result.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, details.Status);
        Assert.Contains("Password", details.Errors.Keys);
        var passwordErrors = Assert.Single(details.Errors, error => error.Key == "Password").Value;
        Assert.Equal("Password must be at least 8 characters.", Assert.Single(passwordErrors));
        Assert.Equal("Validation failed", details.Title);
    }

    [Fact]
    public async Task ValidDto_ContinuesToAction()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<RegisterRequestDto>, RegisterRequestDtoValidator>();
        using var provider = services.BuildServiceProvider();
        var context = CreateContext(provider, new RegisterRequestDto("Petro", "Tymchuk", "petro@example.com", "Abcdef12"));
        var nextCalled = false;
        var filter = new ValidationFilter(provider);

        await filter.OnActionExecutionAsync(context, () =>
        {
            nextCalled = true;
            return Task.FromResult(new ActionExecutedContext(context, [], new object()));
        });

        Assert.True(nextCalled);
        Assert.Null(context.Result);
    }

    private static ActionExecutingContext CreateContext(IServiceProvider services, RegisterRequestDto dto)
    {
        var httpContext = new DefaultHttpContext { RequestServices = services };
        httpContext.Request.Path = "/api/auth/register";
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor(), new ModelStateDictionary());
        return new ActionExecutingContext(actionContext, [], new Dictionary<string, object?> { ["dto"] = dto }, new object());
    }
}
