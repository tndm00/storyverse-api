namespace Content.Application.Interfaces.Services;

/// <summary>
/// The kind of notification the Content service asks the Notification service to
/// create. Maps 1:1 to a <c>NotificationType</c> name on the Notification side;
/// the wire contract carries the name, never an ordinal.
/// </summary>
public enum NotificationKind
{
    ChapterApproved,
    ChapterRejected
}

/// <summary>
/// Sends a notification to the Notification service over HTTP, standing in for
/// the not-yet-built event bus. There is no cross-service transaction: callers
/// invoke this best-effort, after their own work has committed, and swallow
/// failures.
/// </summary>
public interface INotificationServiceClient
{
    /// <summary>
    /// POSTs <c>{BaseUrl}/v1/notifications</c> with the service token header.
    /// </summary>
    Task SendAsync(
        long userId,
        NotificationKind kind,
        string title,
        string body,
        string refType,
        Guid? refId,
        CancellationToken cancellationToken);
}
