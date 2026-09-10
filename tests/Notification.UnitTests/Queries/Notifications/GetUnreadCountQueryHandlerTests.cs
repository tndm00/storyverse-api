using FluentAssertions;
using Notification.Application.Interfaces.Repositories;
using Notification.Application.Interfaces.Services;
using Notification.Application.Queries.Notifications.GetUnreadCount;
using NSubstitute;
using Xunit;

namespace Notification.UnitTests.Queries.Notifications;

public class GetUnreadCountQueryHandlerTests
{
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();

    private readonly GetUnreadCountQueryHandler _handler;

    public GetUnreadCountQueryHandlerTests()
    {
        _handler = new GetUnreadCountQueryHandler(_notificationRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_Should_ReturnUnreadCount_For_CurrentUser()
    {
        _currentUser.GetUserId().Returns(55L);
        _notificationRepository.CountUnreadAsync(55L, Arg.Any<CancellationToken>()).Returns(9);

        var result = await _handler.Handle(new GetUnreadCountQuery(), CancellationToken.None);

        result.Count.Should().Be(9);
        await _notificationRepository.Received(1).CountUnreadAsync(55L, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnZero_When_NoUnreadNotifications()
    {
        _currentUser.GetUserId().Returns(55L);
        _notificationRepository.CountUnreadAsync(55L, Arg.Any<CancellationToken>()).Returns(0);

        var result = await _handler.Handle(new GetUnreadCountQuery(), CancellationToken.None);

        result.Count.Should().Be(0);
    }
}
