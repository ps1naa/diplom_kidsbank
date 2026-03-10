using KidBank.Application.Common.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace KidBank.Infrastructure.Services;

public class RedisSettingsListener : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<RedisSettingsListener> _logger;
    private const string Channel = "settings_changed";

    public RedisSettingsListener(
        IConnectionMultiplexer redis,
        IServiceScopeFactory scopeFactory,
        ILogger<RedisSettingsListener> logger)
    {
        _redis = redis;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var subscriber = _redis.GetSubscriber();

        await subscriber.SubscribeAsync(RedisChannel.Literal(Channel), async (_, message) =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var settingsService = scope.ServiceProvider.GetRequiredService<IAppSettingsService>();
                await settingsService.RefreshCacheAsync(stoppingToken);

                _logger.LogInformation(
                    "Settings cache invalidated due to change from {Source}",
                    message.ToString());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to refresh settings cache after Redis notification");
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
