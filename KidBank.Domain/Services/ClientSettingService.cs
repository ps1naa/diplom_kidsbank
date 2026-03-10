using KidBank.Domain.Entities;

namespace KidBank.Domain.Services;

public static class ClientSettingService
{
    public static void Update(ClientSetting setting, string value)
    {
        setting.Value = value;
        setting.UpdatedAt = DateTime.UtcNow;
    }
}
