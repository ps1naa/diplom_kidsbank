namespace KidBank.Application.Common.Interfaces;

public interface IAuditLogger
{
    Task LogAsync(
        string level,
        string message,
        string? exception = null,
        string? requestPath = null,
        string? requestMethod = null,
        Guid? userId = null,
        string? userEmail = null,
        int? statusCode = null,
        long? elapsedMs = null,
        CancellationToken cancellationToken = default);

    Task LogInfoAsync(string message, CancellationToken cancellationToken = default);
    Task LogWarningAsync(string message, CancellationToken cancellationToken = default);
    Task LogErrorAsync(string message, string? exception = null, CancellationToken cancellationToken = default);
}
