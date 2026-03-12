using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class AuditLogService
{
    public static AuditLog Create(
        string level,
        string message,
        string? exception = null,
        string? requestPath = null,
        string? requestMethod = null,
        Guid? userId = null,
        string? userEmail = null,
        int? statusCode = null,
        long? elapsedMs = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = message,
            Exception = exception,
            RequestPath = requestPath,
            RequestMethod = requestMethod,
            UserId = userId,
            UserEmail = userEmail,
            StatusCode = statusCode,
            ElapsedMs = elapsedMs,
            MachineName = Environment.MachineName
        };
    }
}
