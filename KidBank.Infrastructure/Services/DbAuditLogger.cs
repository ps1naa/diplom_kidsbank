using KidBank.Application.Common.Interfaces;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using KidBank.Infrastructure.Persistence;

namespace KidBank.Infrastructure.Services;

public class DbAuditLogger : IAuditLogger
{
    private readonly ApplicationDbContext _context;
    private readonly IIdentityService _currentUserService;

    public DbAuditLogger(ApplicationDbContext context, IIdentityService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task LogAsync(
        string level,
        string message,
        string? exception = null,
        string? requestPath = null,
        string? requestMethod = null,
        Guid? userId = null,
        string? userEmail = null,
        int? statusCode = null,
        long? elapsedMs = null,
        CancellationToken cancellationToken = default)
    {
        var log = AuditLogService.Create(
            level,
            message,
            exception,
            requestPath,
            requestMethod,
            userId ?? _currentUserService.UserId,
            userEmail,
            statusCode,
            elapsedMs);

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public Task LogInfoAsync(string message, CancellationToken cancellationToken = default)
        => LogAsync("INFO", message, cancellationToken: cancellationToken);

    public Task LogWarningAsync(string message, CancellationToken cancellationToken = default)
        => LogAsync("WARNING", message, cancellationToken: cancellationToken);

    public Task LogErrorAsync(string message, string? exception = null, CancellationToken cancellationToken = default)
        => LogAsync("ERROR", message, exception, cancellationToken: cancellationToken);
}
