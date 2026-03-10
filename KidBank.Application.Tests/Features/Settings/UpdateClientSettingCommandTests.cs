using FluentAssertions;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Features.Settings.Commands;
using KidBank.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace KidBank.Application.Tests.Features.Settings;

public class UpdateClientSettingCommandTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IIdentityService> _identityMock;
    private readonly UpdateClientSettingCommandHandler _handler;

    public UpdateClientSettingCommandTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _identityMock = new Mock<IIdentityService>();
        _handler = new UpdateClientSettingCommandHandler(_contextMock.Object, _identityMock.Object);
    }

    [Fact]
    public async Task Handle_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        _identityMock.Setup(x => x.UserId).Returns((Guid?)null);

        var command = new UpdateClientSettingCommand("theme", "dark");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("UNAUTHORIZED");
    }
}
