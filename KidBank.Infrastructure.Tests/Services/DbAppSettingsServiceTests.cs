using FluentAssertions;
using KidBank.Application.Common.Interfaces;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using KidBank.Infrastructure.Persistence;
using KidBank.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace KidBank.Infrastructure.Tests.Services;

public class DbAppSettingsServiceTests : IDisposable
{
    private readonly SettingsDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly Mock<ISettingsNotifier> _notifierMock;
    private readonly DbAppSettingsService _service;

    public DbAppSettingsServiceTests()
    {
        var options = new DbContextOptionsBuilder<SettingsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new SettingsDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _notifierMock = new Mock<ISettingsNotifier>();
        _service = new DbAppSettingsService(_context, _cache, _notifierMock.Object);
    }

    [Fact]
    public async Task SetForHostAsync_NewSetting_CreatesSetting()
    {
        await _service.SetForHostAsync("test_key", "test_value", Environment.MachineName, "Test description");

        var setting = await _context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == "test_key" && s.Hostname == Environment.MachineName);

        setting.Should().NotBeNull();
        setting!.Value.Should().Be("test_value");
        setting.Description.Should().Be("Test description");
    }

    [Fact]
    public async Task SetAsync_ExistingSetting_UpdatesValue()
    {
        await _service.SetAsync("key1", "old_value");
        await _service.SetAsync("key1", "new_value");

        var setting = await _context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == "key1" && s.Hostname == Environment.MachineName);

        setting!.Value.Should().Be("new_value");
    }

    [Fact]
    public async Task SetAsync_NotifiesSettingsChanged()
    {
        await _service.SetAsync("key1", "value1");

        _notifierMock.Verify(n => n.NotifySettingsChangedAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_HostSpecificSetting_ReturnsHostValue()
    {
        var hostname = Environment.MachineName;
        _context.AppSettings.Add(AppSetting.Create("key1", "host_value", hostname));
        await _context.SaveChangesAsync();

        var result = await _service.GetAsync("key1");

        result.Should().Be("host_value");
    }

    [Fact]
    public async Task GetAsync_GlobalFallback_ReturnsGlobalWhenNoHostSpecific()
    {
        _context.AppSettings.Add(AppSetting.Create("key1", "global_value", AppSetting.GlobalHostname));
        await _context.SaveChangesAsync();

        var result = await _service.GetAsync("key1");

        result.Should().Be("global_value");
    }

    [Fact]
    public async Task GetAsync_HostOverridesGlobal()
    {
        var hostname = Environment.MachineName;
        _context.AppSettings.Add(AppSetting.Create("key1", "global_value", AppSetting.GlobalHostname));
        _context.AppSettings.Add(AppSetting.Create("key1", "host_value", hostname));
        await _context.SaveChangesAsync();

        var result = await _service.GetAsync("key1");

        result.Should().Be("host_value");
    }

    [Fact]
    public async Task GetAsync_NonExistentKey_ReturnsNull()
    {
        var result = await _service.GetAsync("nonexistent");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_Generic_ConvertsToType()
    {
        _context.AppSettings.Add(AppSetting.Create("max_retries", "5", Environment.MachineName));
        await _context.SaveChangesAsync();

        var result = await _service.GetAsync<int>("max_retries");

        result.Should().Be(5);
    }

    [Fact]
    public async Task GetAsync_Generic_InvalidConversion_ReturnsNull()
    {
        _context.AppSettings.Add(AppSetting.Create("key1", "not_a_number", Environment.MachineName));
        await _context.SaveChangesAsync();

        var result = await _service.GetAsync<int>("key1");

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsMergedSettings()
    {
        var hostname = Environment.MachineName;
        _context.AppSettings.Add(AppSetting.Create("global_key", "global_val", AppSetting.GlobalHostname));
        _context.AppSettings.Add(AppSetting.Create("host_key", "host_val", hostname));
        _context.AppSettings.Add(AppSetting.Create("shared_key", "global_shared", AppSetting.GlobalHostname));
        _context.AppSettings.Add(AppSetting.Create("shared_key", "host_shared", hostname));
        await _context.SaveChangesAsync();

        var all = await _service.GetAllAsync();

        all.Should().HaveCount(3);
        all["global_key"].Should().Be("global_val");
        all["host_key"].Should().Be("host_val");
        all["shared_key"].Should().Be("host_shared");
    }

    [Fact]
    public async Task RefreshCacheAsync_InvalidatesCache()
    {
        _context.AppSettings.Add(AppSetting.Create("key1", "old", Environment.MachineName));
        await _context.SaveChangesAsync();

        _ = await _service.GetAsync("key1");

        var entity = await _context.AppSettings.FirstAsync(s => s.Key == "key1");
        AppSettingService.Update(entity, "new");
        await _context.SaveChangesAsync();

        await _service.RefreshCacheAsync();
        var result = await _service.GetAsync("key1");

        result.Should().Be("new");
    }

    public void Dispose()
    {
        _context.Dispose();
        _cache.Dispose();
    }
}
