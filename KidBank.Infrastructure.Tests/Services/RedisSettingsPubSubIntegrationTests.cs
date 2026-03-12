using FluentAssertions;
using KidBank.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using StackExchange.Redis;

namespace KidBank.Infrastructure.Tests.Services;

public class RedisSettingsPubSubIntegrationTests : IAsyncLifetime
{
    private ConnectionMultiplexer _redis = null!;

    public async Task InitializeAsync()
    {
        _redis = await ConnectionMultiplexer.ConnectAsync("localhost:6379");
    }

    [Fact]
    public async Task FullPubSub_NotifierPublishes_ListenerReceives()
    {
        var notifier = new RedisSettingsNotifier(_redis);
        var settingsServiceMock = new Mock<DbAppSettingsService>(null!, null!, null!);
        var scopeFactoryMock = new Mock<IServiceScopeFactory>();
        var scopeMock = new Mock<IServiceScope>();
        var providerMock = new Mock<IServiceProvider>();

        providerMock.Setup(p => p.GetService(typeof(DbAppSettingsService)))
            .Returns(settingsServiceMock.Object);
        scopeMock.Setup(s => s.ServiceProvider).Returns(providerMock.Object);
        scopeFactoryMock.Setup(f => f.CreateScope()).Returns(scopeMock.Object);

        var listener = new RedisSettingsListener(_redis, scopeFactoryMock.Object);

        using var cts = new CancellationTokenSource();
        var listenerTask = listener.StartAsync(cts.Token);

        await Task.Delay(500);
        await notifier.NotifySettingsChangedAsync();
        await Task.Delay(1000);

        settingsServiceMock.Verify(
            s => s.RefreshCacheAsync(It.IsAny<CancellationToken>()),
            Times.AtLeastOnce,
            "Listener should call RefreshCacheAsync when it receives a notification");

        cts.Cancel();
        try { await listener.StopAsync(CancellationToken.None); } catch { }
    }

    public async Task DisposeAsync()
    {
        await _redis.CloseAsync();
        _redis.Dispose();
    }
}
