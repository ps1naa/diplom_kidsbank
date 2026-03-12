using FluentAssertions;
using KidBank.Domain.Entities;
using KidBank.Domain.Services;

namespace KidBank.Domain.Tests.Services;

public class ClientSettingServiceTests
{
    [Fact]
    public void Update_ChangesValueAndTimestamp()
    {
        var setting = ClientSettingService.Create(Guid.NewGuid(), "theme", "light");
        var originalTimestamp = setting.UpdatedAt;

        ClientSettingService.Update(setting, "dark");

        setting.Value.Should().Be("dark");
        setting.UpdatedAt.Should().BeOnOrAfter(originalTimestamp);
    }
}
