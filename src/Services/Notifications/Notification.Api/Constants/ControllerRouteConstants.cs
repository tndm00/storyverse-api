namespace Notification.Api.Constants;

/// <summary>
/// Centralized route segments, per code-standard.md section 11 (Constants Rules)
/// and api-guidelines.md sections 3-4 (Versioning, URI Design).
/// </summary>
public static class ControllerRouteConstants
{
    public const string ApiVersion1 = "v1";

    public const string NotificationsBase = "v1/notifications";

    public const string UnreadCountSegment = "unread-count";
    public const string ReadAllSegment = "read-all";
    public const string NotificationReadSegment = "{notificationId:guid}/read";
}
