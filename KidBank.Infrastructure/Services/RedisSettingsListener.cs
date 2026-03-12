using KidBank.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;


namespace KidBank.Infrastructure.Services;

public class RedisSettingsListener : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private const string Channel = "settings_changed";

    public RedisSettingsListener(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(RedisChannel.Literal(Channel), async (_, message) =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var settingsService = scope.ServiceProvider.GetRequiredService<DbAppSettingsService>();
                await settingsService.RefreshCacheAsync(stoppingToken);

                var auditLogger = scope.ServiceProvider.GetRequiredService<IAuditLogger>();
                await auditLogger.LogInfoAsync(
                    $"Settings cache invalidated due to change from {message}");
            }
            catch (Exception ex)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var auditLogger = scope.ServiceProvider.GetRequiredService<IAuditLogger>();
                    await auditLogger.LogErrorAsync(
                        "Failed to refresh settings cache after Redis notification",
                        ex.ToString());
                }
                catch
                {
                    // ignored
                }
            }
        });

        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            await subscriber.UnsubscribeAsync(RedisChannel.Literal(Channel));
        }
    }
}
