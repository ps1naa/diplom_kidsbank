using FluentAssertions;
using KidBank.Infrastructure.Persistence;
using KidBank.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Npgsql;
using StackExchange.Redis;

namespace KidBank.Infrastructure.Tests.Services;

public class SettingsPlaintextIntegrationTests : IAsyncLifetime
{
    private const string ConnString = "Host=localhost;Port=5432;Database=kidbank_settings;Username=postgres;Password=135790";

    private SettingsDbContext _context = null!;
    private DbAppSettingsService _service = null!;
    private IMemoryCache _cache = null!;

    public Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<SettingsDbContext>()
            .UseNpgsql(ConnString)
            .EnableServiceProviderCaching(false)
            .Options;

        _context = new SettingsDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        var notifier = new Mock<RedisSettingsNotifier>(Mock.Of<IConnectionMultiplexer>());
        _service = new DbAppSettingsService(_context, _cache, notifier.Object);
        return Task.CompletedTask;
    }

    [Fact]
    public async Task SettingsValue_IsStoredAsPlaintext()
    {
        var testKey = $"plain_test_{Guid.NewGuid():N}";
        var plainValue = "Host=db;Port=5432;Database=kidbank;Username=postgres;Password=secret";

        await _service.SetAsync(testKey, plainValue, "testing plaintext storage");

        using var conn = new NpgsqlConnection(ConnString);
        await conn.OpenAsync();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT value FROM app_settings WHERE key = @key AND hostname = @host";
        cmd.Parameters.AddWithValue("key", testKey);
        cmd.Parameters.AddWithValue("host", Environment.MachineName);
        var rawValue = (string?)await cmd.ExecuteScalarAsync();

        rawValue.Should().Be(plainValue, "settings should be stored in plaintext for manual editing");

        await _service.RefreshCacheAsync();
        var retrieved = await _service.GetAsync(testKey);
        retrieved.Should().Be(plainValue);

        _context.AppSettings.RemoveRange(
            _context.AppSettings.Where(s => s.Key == testKey));
        await _context.SaveChangesAsync();
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        _cache.Dispose();
    }
}
