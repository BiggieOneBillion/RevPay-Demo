namespace RevPay.API.Middleware;

public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;

    public IdempotencyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Simple implementation: just pass through for now
        // A real implementation would check headers/cache 
        // but our handler already does that.
        await _next(context);
    }
}
