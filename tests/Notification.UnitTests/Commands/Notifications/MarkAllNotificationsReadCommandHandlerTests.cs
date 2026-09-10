using FluentAssertions;
using Microsoft.Extensions.Logging;
using Notification.Application.Commands.Notifications.MarkAllNotificationsRead;
using Notification.Application.Interfaces.Repositories;
using Notification.Application.Interfaces.Services;
using NSubstitute;
using Xunit;

namespace Notification.UnitTests.Commands.Notifications;

public class MarkAllNotificationsReadCommandHandlerTests
{
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<MarkAllNotificationsReadCommandHandler> _logger =
        Substitute.For<ILogger<MarkAllNotificationsReadCommandHandler>>();

    private readonly MarkAllNotificationsReadCommandHandler _handler;

    public MarkAllNotificationsReadCommandHandlerTests()
    {
        _handler = new MarkAllNotificationsReadCommandHandler(_notificationRepository, _currentUser, _logger);
    }

    [Fact]
    public async Task Handle_Should_MarkAllUnreadNotificationsForCurrentUser_And_ReturnZeroCount()
    {
        _currentUser.GetUserId().Returns(11L);
        _notificationRepository.MarkAllReadAsync(11L, Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(4);

        var result = await _handler.Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None);

        result.Count.Should().Be(0);
        await _notificationRepository.Received(1).MarkAllReadAsync(11L, Arg.Any<DateTime>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ReturnZeroCount_When_NoUnreadNotificationsExist()
    {
        _currentUser.GetUserId().Returns(11L);
        _notificationRepository.MarkAllReadAsync(11L, Arg.Any<DateTime>(), Arg.Any<CancellationToken>()).Returns(0);

        var result = await _handler.Handle(new MarkAllNotificationsReadCommand(), CancellationToken.None);

        result.Count.Should().Be(0);
    }
}
