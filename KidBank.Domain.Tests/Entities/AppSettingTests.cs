using FluentAssertions;
using KidBank.Domain.Entities;

namespace KidBank.Domain.Tests.Entities;

public class AppSettingTests
{
    [Fact]
    public void Create_WithValidArgs_ReturnsAppSetting()
    {
        var setting = AppSetting.Create("jwt:secret", "value123", "host1", "JWT secret key");

        setting.Key.Should().Be("jwt:secret");
        setting.Value.Should().Be("value123");
        setting.Hostname.Should().Be("host1");
        setting.Description.Should().Be("JWT secret key");
        setting.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithGlobalHostname_SetsAsterisk()
    {
        var setting = AppSetting.Create("key1", "val1", AppSetting.GlobalHostname);

        setting.Hostname.Should().Be("*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyKey_ThrowsArgumentException(string? key)
    {
        var act = () => AppSetting.Create(key!, "value", "host");

        act.Should().Throw<ArgumentException>().WithParameterName("key");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyHostname_ThrowsArgumentException(string? hostname)
    {
        var act = () => AppSetting.Create("key", "value", hostname!);

        act.Should().Throw<ArgumentException>().WithParameterName("hostname");
    }

    [Fact]
    public void GlobalHostname_IsAsterisk()
    {
        AppSetting.GlobalHostname.Should().Be("*");
    }
}
