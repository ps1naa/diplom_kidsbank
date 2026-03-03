using System.Text.Json;
using KidBank.Domain.Exceptions;

namespace KidBank.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            DomainException domainEx => new ErrorResponse
            {
                StatusCode = GetStatusCode(domainEx.Code),
                Code = domainEx.Code,
                Message = domainEx.Message
            },
            ArgumentException argEx => new ErrorResponse
            {
                StatusCode = StatusCodes.Status400BadRequest,
                Code = "VALIDATION_ERROR",
                Message = argEx.Message
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                StatusCode = StatusCodes.Status401Unauthorized,
                Code = "UNAUTHORIZED",
                Message = "You are not authorized to perform this action"
            },
            _ => new ErrorResponse
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Code = "INTERNAL_ERROR",
                Message = "An unexpected error occurred"
            }
        };

        context.Response.StatusCode = response.StatusCode;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, options));
    }

    private static int GetStatusCode(string code) => code switch
    {
        "NOT_FOUND" => StatusCodes.Status404NotFound,
        "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
        "FORBIDDEN" => StatusCodes.Status403Forbidden,
        "VALIDATION_ERROR" => StatusCodes.Status400BadRequest,
        "CONFLICT" => StatusCodes.Status409Conflict,
        "INSUFFICIENT_FUNDS" => StatusCodes.Status400BadRequest,
        "SPENDING_LIMIT_EXCEEDED" => StatusCodes.Status400BadRequest,
        "CONCURRENCY_CONFLICT" => StatusCodes.Status409Conflict,
        "INVALID_OPERATION" => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError
    };
}

public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
