namespace Notification.Application.Constants;

/// <summary>
/// Structured-logging message templates for the Notification service. No secrets
/// or notification body text in these templates, per code-standard.md section 12.
/// </summary>
public static class ApplicationLogConstants
{
    public const string NotificationCreated = "Notification {NotificationId} of type {NotificationType} created for user {UserId}.";
    public const string NotificationRead = "Notification {NotificationId} marked read by user {UserId}.";
    public const string NotificationsAllRead = "User {UserId} marked {Count} notifications read.";
}
