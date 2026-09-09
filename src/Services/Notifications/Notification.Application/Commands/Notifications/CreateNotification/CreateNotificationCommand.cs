namespace Notification.Application.Commands.Notifications.CreateNotification;

/// <summary>
/// Creates one notification for a recipient. This is the domain logic that an
/// inbound integration event (ChapterPublished / CommentReplied / ReportResolved)
/// would invoke once the event bus is implemented; today it is reachable only
/// through the internal creation endpoint.
/// </summary>
public sealed class CreateNotificationCommand : ICommand<NotificationResponseDto>
{
    public long UserId { get; init; }

    public NotificationType Type { get; init; }

    public string Title { get; init; } = string.Empty;

    public string Body { get; init; } = string.Empty;

    public string RefType { get; init; }

    public Guid? RefId { get; init; }
}
