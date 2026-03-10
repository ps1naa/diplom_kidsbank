using KidBank.Application.Common.Interfaces;
using StackExchange.Redis;

namespace KidBank.Infrastructure.Services;

public class RedisSettingsNotifier : ISettingsNotifier
{
    private readonly IConnectionMultiplexer _redis;
    private const string Channel = "settings_changed";

    public RedisSettingsNotifier(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task NotifySettingsChangedAsync(CancellationToken cancellationToken = default)
    {
        var subscriber = _redis.GetSubscriber();
        await subscriber.PublishAsync(RedisChannel.Literal(Channel), Environment.MachineName);
    }
}
