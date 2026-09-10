using FluentAssertions;
using Microsoft.Extensions.Logging;
using Notification.Application.Commands.Notifications.CreateNotification;
using Notification.Application.Interfaces.Repositories;
using Notification.Domain.Enums;
using NSubstitute;
using Xunit;
using NotificationEntity = Notification.Domain.Entities.Notification;

namespace Notification.UnitTests.Commands.Notifications;

public class CreateNotificationCommandHandlerTests
{
    private readonly INotificationRepository _notificationRepository = Substitute.For<INotificationRepository>();
    private readonly ILogger<CreateNotificationCommandHandler> _logger =
        Substitute.For<ILogger<CreateNotificationCommandHandler>>();

    private readonly CreateNotificationCommandHandler _handler;

    public CreateNotificationCommandHandlerTests()
    {
        _handler = new CreateNotificationCommandHandler(_notificationRepository, _logger);
    }

    [Fact]
    public async Task Handle_Should_PersistNotification_And_ReturnMappedDto()
    {
        var command = new CreateNotificationCommand
        {
            UserId = 42,
            Type = NotificationType.CommentReply,
            Title = "  Someone replied  ",
            Body = "  Check it out  ",
            RefType = "comment",
            RefId = Guid.NewGuid()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Title.Should().Be("Someone replied");
        result.Body.Should().Be("Check it out");
        result.Type.Should().Be(NotificationType.CommentReply.ToString());
        result.IsRead.Should().BeFalse();
        await _notificationRepository.Received(1).AddAsync(Arg.Any<NotificationEntity>(), Arg.Any<CancellationToken>());
        await _notificationRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_Should_ClearRefType_When_RefTypeIsWhitespace()
    {
        var command = new CreateNotificationCommand
        {
            UserId = 1,
            Type = NotificationType.SystemAnnouncement,
            Title = "Title",
            Body = "Body",
            RefType = "   ",
            RefId = null
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.RefType.Should().BeNull();
    }

    // KNOWN, ACCEPTED gap: this handler performs no ownership check on UserId. Any
    // authenticated caller can create a notification addressed to any recipient.
    // The frontend's comment-reply feature relies on this cross-user creation path
    // as a stand-in for a not-yet-built event bus (see the XML doc on
    // CreateNotificationCommand). This test does not assert that the behavior
    // *should* remain this way forever — it exists purely so that an accidental
    // future change to this behavior (e.g. someone adding an ownership check)
    // shows up as a visible diff here, prompting a deliberate decision rather
    // than a silent regression.
    [Fact]
    public async Task Handle_Should_AllowCreatingNotificationForAnyUserId_With_NoOwnershipCheck()
    {
        var command = new CreateNotificationCommand
        {
            UserId = 999, // arbitrary recipient, unrelated to any "current caller"
            Type = NotificationType.CommentReply,
            Title = "Reply",
            Body = "Body",
            RefType = "comment",
            RefId = Guid.NewGuid()
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        await _notificationRepository.Received(1).AddAsync(
            Arg.Is<NotificationEntity>(n => n.UserId == 999),
            Arg.Any<CancellationToken>());
    }
}
