using KidBank.Application.Common.Interfaces;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using KidBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace KidBank.Infrastructure.Services;

public class DbAppSettingsService : IAppSettingsService
{
    private readonly SettingsDbContext _context;
    private readonly IMemoryCache _cache;
    private readonly ISettingsNotifier _notifier;
    private readonly string _hostname;
    private const string CachePrefix = "AppSettings_";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public DbAppSettingsService(SettingsDbContext context, IMemoryCache cache, ISettingsNotifier notifier)
    {
        _context = context;
        _cache = cache;
        _notifier = notifier;
        _hostname = Environment.MachineName;
    }

    public async Task<string?> GetAsync(string key, CancellationToken cancellationToken = default)
    {
        var settings = await GetCachedSettingsAsync(cancellationToken);
        return settings.TryGetValue(key, out var value) ? value : null;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : struct
    {
        var value = await GetAsync(key, cancellationToken);
        if (value == null) return null;

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return null;
        }
    }

    public Task SetAsync(string key, string value, string? description = null, CancellationToken cancellationToken = default)
    {
        return SetForHostAsync(key, value, _hostname, description, cancellationToken);
    }

    public async Task SetForHostAsync(string key, string value, string hostname, string? description = null, CancellationToken cancellationToken = default)
    {
        var existing = await _context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == key && s.Hostname == hostname, cancellationToken);

        if (existing != null)
        {
            AppSettingService.Update(existing, value, description);
        }
        else
        {
            var setting = AppSetting.Create(key, value, hostname, description);
            _context.AppSettings.Add(setting);
        }

        await _context.SaveChangesAsync(cancellationToken);
        InvalidateCache();
        await _notifier.NotifySettingsChangedAsync(cancellationToken);
    }

    public async Task<Dictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetCachedSettingsAsync(cancellationToken);
    }

    public Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        InvalidateCache();
        return Task.CompletedTask;
    }

    private void InvalidateCache()
    {
        _cache.Remove(CachePrefix + _hostname);
    }

    private async Task<Dictionary<string, string>> GetCachedSettingsAsync(CancellationToken cancellationToken)
    {
        var cacheKey = CachePrefix + _hostname;

        if (_cache.TryGetValue<Dictionary<string, string>>(cacheKey, out var cached) && cached != null)
            return cached;

        var allSettings = await _context.AppSettings
            .AsNoTracking()
            .Where(s => s.Hostname == _hostname || s.Hostname == AppSetting.GlobalHostname)
            .ToListAsync(cancellationToken);

        var result = new Dictionary<string, string>();

        foreach (var setting in allSettings.Where(s => s.Hostname == AppSetting.GlobalHostname))
            result[setting.Key] = setting.Value;

        foreach (var setting in allSettings.Where(s => s.Hostname == _hostname))
            result[setting.Key] = setting.Value;

        _cache.Set(cacheKey, result, CacheDuration);
        return result;
    }
}
