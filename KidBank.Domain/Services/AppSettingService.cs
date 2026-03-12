using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class AppSettingService
{
    public static AppSetting Create(string key, string value, string hostname, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));
        if (string.IsNullOrWhiteSpace(hostname))
            throw new ArgumentException("Hostname cannot be empty", nameof(hostname));

        return new AppSetting
        {
            Key = key,
            Hostname = hostname,
            Value = value,
            Description = description,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static void Update(AppSetting setting, string value, string? description = null)
    {
        setting.Value = value;
        if (description != null)
            setting.Description = description;
        setting.UpdatedAt = DateTime.UtcNow;
    }
}
