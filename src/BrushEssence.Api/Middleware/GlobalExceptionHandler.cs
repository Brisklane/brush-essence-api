using BrushEssence.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace BrushEssence.Api.Middleware;

/// <summary>
/// Centralized exception handling. Implements the .NET 9 <see cref="IExceptionHandler"/>
/// abstraction (registered via <c>AddExceptionHandler</c> + <c>UseExceptionHandler</c>)
/// and converts exceptions into RFC 7807 <see cref="ProblemDetails"/>.
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
        var (statusCode, title) = exception switch
        {
            ValidationException => (StatusCodes.Status400BadRequest, "One or more validation errors occurred."),
            BadRequestException => (StatusCodes.Status400BadRequest, "The request could not be processed."),
            AuthenticationException => (StatusCodes.Status401Unauthorized, "Authentication failed."),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Authentication is required."),
            NotFoundException or KeyNotFoundException => (StatusCodes.Status404NotFound, "The requested resource was not found."),
            ConflictException => (StatusCodes.Status409Conflict, "The request conflicts with the current state."),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };

        // Expected (mappable) errors are routine; only unexpected 500s are logged as errors.
        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception processing {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }
        else
        {
            _logger.LogWarning(
                "Handled {ExceptionType} on {Method} {Path}: {Message}",
                exception.GetType().Name,
                httpContext.Request.Method,
                httpContext.Request.Path,
                exception.Message);
        }

        httpContext.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = $"https://httpstatuses.io/{statusCode}",
            Instance = $"{httpContext.Request.Method} {httpContext.Request.Path}",
        };

        // Correlation id so a client-reported error can be traced in the logs.
        problemDetails.Extensions["traceId"] =
            System.Diagnostics.Activity.Current?.Id ?? httpContext.TraceIdentifier;

        // Surface the message for expected application errors; never leak internals on 500.
        if (exception is AppException)
        {
            problemDetails.Detail = exception.Message;
        }

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
