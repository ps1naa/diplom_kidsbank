namespace KidBank.Application.Common.Interfaces;

public interface IAppSettingsService
{
    Task<string?> GetAsync(string key, CancellationToken cancellationToken = default);
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : struct;
    Task SetAsync(string key, string value, string? description = null, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> GetAllAsync(CancellationToken cancellationToken = default);
    Task RefreshCacheAsync(CancellationToken cancellationToken = default);
}
