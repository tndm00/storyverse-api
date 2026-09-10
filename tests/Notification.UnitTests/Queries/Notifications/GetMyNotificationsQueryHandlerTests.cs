using FluentAssertions;
using Notification.Application.Interfaces.Repositories;
using Notification.Application.Interfaces.Services;
using Notification.Application.Queries.Notifications.GetMyNotifications;
using Notification.Domain.Enums;
using NSubstitute;
using Xunit;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.UnitTests.Queries.Notifications;

public class GetMyNotificationsQueryHandlerTests
{
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();

    private readonly GetMyNotificationsQueryHandler _handler;

    public GetMyNotificationsQueryHandlerTests()
    {
        _handler = new GetMyNotificationsQueryHandler(_notificationRepository, _currentUser);
    }

    [Fact]
    public async Task Handle_Should_PassIsReadFilter_To_Repository_When_Specified()
    {
        _currentUser.GetUserId().Returns(3L);
        _notificationRepository.GetForUserAsync(3L, true, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<NotificationEntity>
            {
                new() { PublicId = Guid.NewGuid(), UserId = 3, IsRead = true, Type = NotificationType.NewChapter }
            }, 1));

        var query = new GetMyNotificationsQuery { IsRead = true, PageNumber = 1, PageSize = 20 };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().ContainSingle();
        result.Items[0].IsRead.Should().BeTrue();
        await _notificationRepository.Received(1).GetForUserAsync(3L, true, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_PassNullIsReadFilter_When_NotSpecified()
    {
        _currentUser.GetUserId().Returns(3L);
        _notificationRepository.GetForUserAsync(3L, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<NotificationEntity>(), 0));

        var query = new GetMyNotificationsQuery { IsRead = null, PageNumber = 1, PageSize = 20 };

        await _handler.Handle(query, CancellationToken.None);

        await _notificationRepository.Received(1).GetForUserAsync(3L, null, 1, 20, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_OnlyReturnUnread_When_IsReadFilterIsFalse()
    {
        _currentUser.GetUserId().Returns(3L);
        var unread = new NotificationEntity { PublicId = Guid.NewGuid(), UserId = 3, IsRead = false };
        _notificationRepository.GetForUserAsync(3L, false, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<NotificationEntity> { unread }, 1));

        var query = new GetMyNotificationsQuery { IsRead = false };

        var result = await _handler.Handle(query, CancellationToken.None);

        result.Items.Should().OnlyContain(x => !x.IsRead);
    }

    [Fact]
    public async Task Handle_Should_ClampPageNumberToOne_When_RequestedPageIsNotPositive()
    {
        _currentUser.GetUserId().Returns(3L);
        _notificationRepository.GetForUserAsync(3L, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<NotificationEntity>(), 0));

        var query = new GetMyNotificationsQuery { PageNumber = 0, PageSize = 20 };

        await _handler.Handle(query, CancellationToken.None);

        await _notificationRepository.Received(1).GetForUserAsync(3L, null, 1, 20, Arg.Any<CancellationToken>());
    }
}
