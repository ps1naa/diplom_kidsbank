using StackExchange.Redis;

namespace KidBank.Infrastructure.Services;

public class RedisSettingsNotifier
{
    private readonly IConnectionMultiplexer _redis;
    private const string Channel = "settings_changed";

    public RedisSettingsNotifier(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public virtual async Task NotifySettingsChangedAsync(CancellationToken cancellationToken = default)
    {
        var subscriber = _redis.GetSubscriber();
        await subscriber.PublishAsync(RedisChannel.Literal(Channel), Environment.MachineName);
    }
}
