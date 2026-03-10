namespace KidBank.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Level { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? Exception { get; private set; }
    public string? RequestPath { get; private set; }
    public string? RequestMethod { get; private set; }
    public Guid? UserId { get; private set; }
    public string? UserEmail { get; private set; }
    public int? StatusCode { get; private set; }
    public long? ElapsedMs { get; private set; }
    public string? MachineName { get; private set; }

    private AuditLog() { }

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
