using FluentAssertions;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;

namespace KidBank.Domain.Tests.Services;

public class AppSettingServiceTests
{
    [Fact]
    public void Update_ChangesValueAndTimestamp()
    {
        var setting = AppSettingService.Create("key", "old_value", "*");
        var originalTimestamp = setting.UpdatedAt;

        AppSettingService.Update(setting, "new_value");

        setting.Value.Should().Be("new_value");
        setting.UpdatedAt.Should().BeOnOrAfter(originalTimestamp);
    }

    [Fact]
    public void Update_WithDescription_ChangesDescription()
    {
        var setting = AppSettingService.Create("key", "value", "*", "old desc");

        AppSettingService.Update(setting, "new_value", "new desc");

        setting.Description.Should().Be("new desc");
    }

    [Fact]
    public void Update_WithNullDescription_KeepsExistingDescription()
    {
        var setting = AppSettingService.Create("key", "value", "*", "existing desc");

        AppSettingService.Update(setting, "new_value");

        setting.Description.Should().Be("existing desc");
    }
}
