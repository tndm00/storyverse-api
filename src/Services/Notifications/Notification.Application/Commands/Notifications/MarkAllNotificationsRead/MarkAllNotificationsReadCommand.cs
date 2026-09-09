namespace Notification.Application.Commands.Notifications.MarkAllNotificationsRead;

/// <summary>Marks every unread notification owned by the current user as read.</summary>
public sealed class MarkAllNotificationsReadCommand : ICommand<UnreadCountResponseDto>
{
}
