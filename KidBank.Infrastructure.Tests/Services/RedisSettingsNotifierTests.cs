using FluentAssertions;
using KidBank.Infrastructure.Services;
using StackExchange.Redis;

namespace KidBank.Infrastructure.Tests.Services;

public class RedisSettingsNotifierTests : IAsyncLifetime
{
    private ConnectionMultiplexer _redis = null!;
    private RedisSettingsNotifier _notifier = null!;

    public async Task InitializeAsync()
    {
        _redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        _notifier = new RedisSettingsNotifier(_redis);
    }

    [Fact]
    public async Task NotifySettingsChangedAsync_PublishesMessage()
    {
        var subscriber = _redis.GetSubscriber();
        var received = new TaskCompletionSource<string>();

        await subscriber.SubscribeAsync(RedisChannel.Literal("settings_changed"), (_, message) =>
        {
            received.TrySetResult(message!);
        });

        await Task.Delay(100);
        await _notifier.NotifySettingsChangedAsync();

        var result = await Task.WhenAny(received.Task, Task.Delay(5000));
        result.Should().Be(received.Task, "message should be received within timeout");

        var msg = await received.Task;
        msg.Should().Be(Environment.MachineName);

        await subscriber.UnsubscribeAsync(RedisChannel.Literal("settings_changed"));
    }

    public async Task DisposeAsync()
    {
        await _redis.CloseAsync();
        _redis.Dispose();
    }
}
