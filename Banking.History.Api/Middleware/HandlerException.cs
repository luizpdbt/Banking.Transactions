using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public class HandlerException(
    ILogger<HandlerException> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            ArgumentException =>
                (StatusCodes.Status400BadRequest, "Invalid request"),

            KeyNotFoundException =>
                (StatusCodes.Status404NotFound, "Resource not found"),

            InvalidOperationException =>
                (StatusCodes.Status409Conflict, "Operation conflict"),

            _ =>
                (StatusCodes.Status500InternalServerError, "Internal server error")
        };

        

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = "An unexpected error occurred.",
            Instance = httpContext.Request.Path
        };

        logger.LogError(exception,"Exception occurred. TraceId: {TraceId}",httpContext.TraceIdentifier);


        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problem,cancellationToken);

        return true;
    }
}