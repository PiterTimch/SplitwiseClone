namespace SplitwiseClone.Api.Exceptions;

using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using SplitwiseClone.Domain.Exceptions;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred while processing the request, on {Path}: {Message}", httpContext.Request.Path, exception.Message);

        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "Validation Error"),
            NotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            ConflictException => (StatusCodes.Status409Conflict, "Business Conflict"),
            Domain.Exceptions.UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized Access"),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        object response;

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            response = new { status = statusCode, title, errors };
        }
        else
        {
            response = new { status = statusCode, title, detail = exception.Message };
        }

        await httpContext.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions), cancellationToken);

        return true;
    }
}
