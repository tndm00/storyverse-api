using Be.StoryVerse.Core.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Notification.Application.Commands.Notifications.MarkNotificationRead;
using Notification.Application.Interfaces.Repositories;
using Notification.Application.Interfaces.Services;
using NSubstitute;
using Xunit;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.UnitTests.Commands.Notifications;

public class MarkNotificationReadCommandHandlerTests
{
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly ICurrentUserContext _currentUser = Substitute.For<ICurrentUserContext>();
    private readonly ILogger<MarkNotificationReadCommandHandler> _logger =
        Substitute.For<ILogger<MarkNotificationReadCommandHandler>>();

    private readonly MarkNotificationReadCommandHandler _handler;

    public MarkNotificationReadCommandHandlerTests()
    {
        _handler = new MarkNotificationReadCommandHandler(_notificationRepository, _currentUser, _logger);
    }

    [Fact]
    public async Task Handle_Should_MarkNotificationRead_When_ItBelongsToCurrentUser()
    {
        var publicId = Guid.NewGuid();
        var notification = new NotificationEntity { PublicId = publicId, UserId = 7, IsRead = false };

        _currentUser.GetUserId().Returns(7L);
        _notificationRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(notification);

        var command = new MarkNotificationReadCommand { NotificationId = publicId };

        var result = await _handler.Handle(command, CancellationToken.None);

        notification.IsRead.Should().BeTrue();
        notification.ReadAt.Should().NotBeNull();
        result.IsRead.Should().BeTrue();
        _notificationRepository.Received(1).Update(notification);
        await _notificationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_NotTouchRepository_When_NotificationAlreadyRead()
    {
        var publicId = Guid.NewGuid();
        var readAt = DateTime.UtcNow.AddDays(-1);
        var notification = new NotificationEntity
        {
            PublicId = publicId,
            UserId = 7,
            IsRead = true,
            ReadAt = readAt
        };

        _currentUser.GetUserId().Returns(7L);
        _notificationRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(notification);

        var command = new MarkNotificationReadCommand { NotificationId = publicId };

        await _handler.Handle(command, CancellationToken.None);

        notification.ReadAt.Should().Be(readAt);
        _notificationRepository.DidNotReceive().Update(Arg.Any<NotificationEntity>());
        await _notificationRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_When_NotificationDoesNotExist()
    {
        var publicId = Guid.NewGuid();

        _currentUser.GetUserId().Returns(7L);
        _notificationRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>())
            .Returns((NotificationEntity)null);

        var command = new MarkNotificationReadCommand { NotificationId = publicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    // Regression test: a notification owned by a different user must look
    // exactly like a missing one (NotFoundException), never a ForbiddenException.
    // Returning "forbidden" would leak to the caller that a notification with
    // that id exists but belongs to someone else, letting them probe another
    // user's feed one guess at a time (see the handler's own comment on this).
    [Fact]
    public async Task Handle_Should_ThrowNotFoundException_NotForbidden_When_NotificationBelongsToDifferentUser()
    {
        var publicId = Guid.NewGuid();
        var notification = new NotificationEntity { PublicId = publicId, UserId = 123, IsRead = false };

        _currentUser.GetUserId().Returns(999L);
        _notificationRepository.GetByPublicIdAsync(publicId, Arg.Any<CancellationToken>()).Returns(notification);

        var command = new MarkNotificationReadCommand { NotificationId = publicId };

        Func<Task> act = () => _handler.Handle(command, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<NotFoundException>();
        exception.Which.Should().NotBeOfType<ForbiddenException>();
        notification.IsRead.Should().BeFalse();
        _notificationRepository.DidNotReceive().Update(Arg.Any<NotificationEntity>());
    }
}
