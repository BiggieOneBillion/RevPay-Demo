using Microsoft.AspNetCore.Diagnostics;
using System.Diagnostics;
using System.Text.Json;
using RevPay.Application.Common.Exceptions;
using FluentValidation;

namespace RevPay.API.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext ctx,
        Exception exception, CancellationToken ct)
    {
        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var (statusCode, message) = exception switch
        {
            ValidationException ve => (400, string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),
            NotFoundException    => (404, exception.Message),
            ForbiddenException   => (403, exception.Message),
            BusinessRuleException=> (422, exception.Message),
            _                    => (500, "An unexpected error occurred.")
        };

        ctx.Response.StatusCode = statusCode;
        await ctx.Response.WriteAsJsonAsync(new
        {
            success = false,
            message,
            traceId = Activity.Current?.Id ?? ctx.TraceIdentifier
        }, ct);

        return true;
    }
}
