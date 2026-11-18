using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using SplitwiseClone.Api.Exceptions;
using SplitwiseClone.Application.Exceptions;
using Xunit;

namespace SplitwiseClone.Application.Tests;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task ValidationFailure_ReturnsProblemDetailsWithErrors()
    {
        var result = await Invoke(new ValidationException([new FluentValidation.Results.ValidationFailure("Name", "Name is required")]), "/api/groups");

        Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
        Assert.StartsWith("application/problem+json", result.ContentType);
        using var json = JsonDocument.Parse(result.Body);
        Assert.Equal(400, json.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Name is required", json.RootElement.GetProperty("errors").GetProperty("Name")[0].GetString());
    }

    [Fact]
    public async Task NotFoundFailure_Returns404ProblemDetails()
    {
        var result = await Invoke(new EntityNotFoundException("Group", 99), "/api/groups/99");

        Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
        Assert.StartsWith("application/problem+json", result.ContentType);
    }

    [Fact]
    public async Task UnexpectedFailure_DoesNotExposeExceptionMessage()
    {
        var result = await Invoke(new InvalidOperationException("database password leaked"), "/api/groups");

        Assert.Equal(StatusCodes.Status500InternalServerError, result.StatusCode);
        Assert.DoesNotContain("database password leaked", result.Body);
        Assert.Contains("internal error", result.Body, StringComparison.OrdinalIgnoreCase);
    }

    private static async Task<(int StatusCode, string? ContentType, string Body)> Invoke(Exception exception, string path)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        await using var provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = provider };
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        var handler = new GlobalExceptionHandler(provider.GetRequiredService<IProblemDetailsService>(),
            provider.GetRequiredService<Microsoft.Extensions.Logging.ILogger<GlobalExceptionHandler>>());

        Assert.True(await handler.TryHandleAsync(context, exception, CancellationToken.None));
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return (context.Response.StatusCode, context.Response.ContentType, await reader.ReadToEndAsync());
    }
}
