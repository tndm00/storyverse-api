namespace Notification.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string ConnectionStringName = "NotificationDatabase";

    /// <summary>
    /// Business-boundary database schema for notification tables, per
    /// code-standard.md section 21 (Database Schema Rules).
    /// </summary>
    public const string NotificationSchemaName = "notification";
}
