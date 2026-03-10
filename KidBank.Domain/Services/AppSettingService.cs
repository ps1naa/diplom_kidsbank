using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class AppSettingService
{
    public static void Update(AppSetting setting, string value, string? description = null)
    {
        setting.Value = value;
        if (description != null)
            setting.Description = description;
        setting.UpdatedAt = DateTime.UtcNow;
    }
}
