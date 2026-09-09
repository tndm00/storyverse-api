namespace Notification.Application.Commands.Notifications.MarkNotificationRead;

/// <summary>Marks a single notification owned by the current user as read.</summary>
public sealed class MarkNotificationReadCommand : ICommand<NotificationResponseDto>
{
    public Guid NotificationId { get; init; }
}
