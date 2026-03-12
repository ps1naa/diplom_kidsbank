using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class ClientSettingService
{
    public static ClientSetting Create(Guid userId, string key, string value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be empty", nameof(key));

        return new ClientSetting
        {
            UserId = userId,
            Key = key,
            Value = value,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public static void Update(ClientSetting setting, string value)
    {
        setting.Value = value;
        setting.UpdatedAt = DateTime.UtcNow;
    }
}
