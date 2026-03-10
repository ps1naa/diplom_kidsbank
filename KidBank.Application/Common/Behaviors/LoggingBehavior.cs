using System.Diagnostics;
using KidBank.Application.Common.Interfaces;
using MediatR;

namespace KidBank.Application.Common.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IAuditLogger _auditLogger;
    private readonly IIdentityService _currentUserService;

    public LoggingBehavior(
        IAuditLogger auditLogger,
        IIdentityService currentUserService)
    {
        _auditLogger = auditLogger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId;

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            await _auditLogger.LogAsync(
                "INFO",
                $"Handled {requestName}",
                userId: userId,
                elapsedMs: stopwatch.ElapsedMilliseconds,
                cancellationToken: cancellationToken);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            await _auditLogger.LogAsync(
                "ERROR",
                $"Error handling {requestName}",
                exception: ex.ToString(),
                userId: userId,
                elapsedMs: stopwatch.ElapsedMilliseconds,
                cancellationToken: cancellationToken);

            throw;
        }
    }
}
