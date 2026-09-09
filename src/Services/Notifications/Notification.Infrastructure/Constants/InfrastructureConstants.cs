namespace Notification.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string ConnectionStringName = "NotificationDatabase";

    /// <summary>
    /// Business-boundary database schema for notification tables, per
    /// code-standard.md section 21 (Database Schema Rules).
    /// </summary>
    public const string NotificationSchemaName = "notification";

    public const string NotificationsTableName = "notifications";

    /// <summary>
    /// Fallback JWT subject claim name, used when the inbound claim was not
    /// remapped to <see cref="System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub"/>.
    /// </summary>
    public const string SubClaimType = "sub";

    /// <summary>Design-time override for the migration-scaffolding connection string.</summary>
    public const string DesignTimeConnectionEnvVar = "NOTIFICATION_DB_CONNECTION";

    public const int EnumColumnLength = 30;
}
