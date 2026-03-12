using FluentAssertions;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using KidBank.Infrastructure.Persistence;
using KidBank.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using StackExchange.Redis;

namespace KidBank.Infrastructure.Tests.Services;

public class DbAppSettingsServiceIntegrationTests : IAsyncLifetime
{
    private const string ConnString = "Host=localhost;Port=5432;Database=kidbank_settings;Username=postgres;Password=135790";

    private SettingsDbContext _context = null!;
    private DbAppSettingsService _service = null!;
    private IMemoryCache _cache = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<SettingsDbContext>()
            .UseNpgsql(ConnString)
            .EnableServiceProviderCaching(false)
            .Options;

        _context = new SettingsDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        var notifier = new Mock<RedisSettingsNotifier>(Mock.Of<IConnectionMultiplexer>());
        _service = new DbAppSettingsService(_context, _cache, notifier.Object);

        await _context.Database.EnsureCreatedAsync();
    }

    [Fact]
    public async Task RealDb_HostOverridesGlobal()
    {
        var hostname = Environment.MachineName;
        var testKey = $"integration_test_{Guid.NewGuid():N}";

        _context.AppSettings.Add(AppSettingService.Create(testKey, "global_val", AppSetting.GlobalHostname));
        _context.AppSettings.Add(AppSettingService.Create(testKey, "host_val", hostname));
        await _context.SaveChangesAsync();

        var result = await _service.GetAsync(testKey);

        result.Should().Be("host_val", "host-specific should override global");

        _context.AppSettings.RemoveRange(
            _context.AppSettings.Where(s => s.Key == testKey));
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task RealDb_FallbackToGlobal()
    {
        var testKey = $"integration_test_{Guid.NewGuid():N}";

        _context.AppSettings.Add(AppSettingService.Create(testKey, "global_only", AppSetting.GlobalHostname));
        await _context.SaveChangesAsync();

        var result = await _service.GetAsync(testKey);

        result.Should().Be("global_only", "should fall back to global when no host-specific");

        _context.AppSettings.RemoveRange(
            _context.AppSettings.Where(s => s.Key == testKey));
        await _context.SaveChangesAsync();
    }

    [Fact]
    public async Task RealDb_SetAndGetRoundTrip()
    {
        var testKey = $"integration_test_{Guid.NewGuid():N}";

        await _service.SetAsync(testKey, "written_value", "test setting");

        await _service.RefreshCacheAsync();
        var result = await _service.GetAsync(testKey);

        result.Should().Be("written_value");

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
