using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Middleware;

/// <summary>
/// Centralized exception handling. Implements the .NET 9 <see cref="IExceptionHandler"/>
/// abstraction (registered via <c>AddExceptionHandler</c> + <c>UseExceptionHandler</c>)
/// and converts unhandled exceptions into RFC 7807 <see cref="ProblemDetails"/>.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetailsService;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetailsService = problemDetailsService;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception processing {Method} {Path}",
            httpContext.Request.Method,
            httpContext.Request.Path);

        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "One or more validation errors occurred."),
            KeyNotFoundException => (StatusCodes.Status404NotFound, "The requested resource was not found."),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Authentication is required."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
        };

        if (exception is ValidationException validationException)
        {
            problemDetails.Extensions["errors"] = validationException.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());
        }

        return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        });
    }
}
