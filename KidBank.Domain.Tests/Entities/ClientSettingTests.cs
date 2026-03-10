using FluentAssertions;
using KidBank.Domain.Entities;

namespace KidBank.Domain.Tests.Entities;

public class ClientSettingTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsClientSetting()
    {
        var userId = Guid.NewGuid();
        var setting = ClientSetting.Create(userId, "theme", "dark");

        setting.UserId.Should().Be(userId);
        setting.Key.Should().Be("theme");
        setting.Value.Should().Be("dark");
        setting.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyKey_ThrowsArgumentException(string? key)
    {
        var act = () => ClientSetting.Create(Guid.NewGuid(), key!, "value");

        act.Should().Throw<ArgumentException>().WithParameterName("key");
    }
}
