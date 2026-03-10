using KidBank.Application.Common.Interfaces;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;
using KidBank.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace KidBank.Infrastructure.Services;

public class DbAppSettingsService : IAppSettingsService
{
    private readonly ApplicationDbContext _context;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "AppSettings";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public DbAppSettingsService(ApplicationDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
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

    public async Task SetAsync(string key, string value, string? description = null, CancellationToken cancellationToken = default)
    {
        var existing = await _context.AppSettings
            .FirstOrDefaultAsync(s => s.Key == key, cancellationToken);

        if (existing != null)
        {
            AppSettingService.Update(existing, value, description);
        }
        else
        {
            var setting = AppSetting.Create(key, value, description);
            _context.AppSettings.Add(setting);
        }

        await _context.SaveChangesAsync(cancellationToken);
        _cache.Remove(CacheKey);
    }

    public async Task<Dictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await GetCachedSettingsAsync(cancellationToken);
    }

    public Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        _cache.Remove(CacheKey);
        return Task.CompletedTask;
    }

    private async Task<Dictionary<string, string>> GetCachedSettingsAsync(CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue<Dictionary<string, string>>(CacheKey, out var cached) && cached != null)
        {
            return cached;
        }

        var settings = await _context.AppSettings
            .AsNoTracking()
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

        _cache.Set(CacheKey, settings, CacheDuration);
        return settings;
    }
}
