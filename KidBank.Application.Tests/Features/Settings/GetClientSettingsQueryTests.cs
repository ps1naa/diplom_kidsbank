using FluentAssertions;
using KidBank.Application.Common.Interfaces;
using KidBank.Application.Features.Settings.Queries;
using Moq;

namespace KidBank.Application.Tests.Features.Settings;

public class GetClientSettingsQueryTests
{
    [Fact]
    public async Task Handle_WhenNotAuthenticated_ReturnsUnauthorized()
    {
        var contextMock = new Mock<IApplicationDbContext>();
        var identityMock = new Mock<IIdentityService>();
        identityMock.Setup(x => x.UserId).Returns((Guid?)null);

        var handler = new GetClientSettingsQueryHandler(contextMock.Object, identityMock.Object);
        var result = await handler.Handle(new GetClientSettingsQuery(), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("UNAUTHORIZED");
    }
}
