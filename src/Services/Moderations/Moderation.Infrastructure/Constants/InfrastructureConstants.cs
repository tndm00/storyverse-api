namespace Moderation.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string ConnectionStringName = "ModerationDatabase";

    /// <summary>
    /// Business-boundary database schema for moderation tables, per
    /// code-standard.md section 21 (Database Schema Rules).
    /// </summary>
    public const string ModerationSchemaName = "moderation";

    public const string ReportsTableName = "reports";
    public const string ModerationActionsTableName = "moderation_actions";

    /// <summary>
    /// Fallback JWT subject claim name, used when the inbound claim was not
    /// remapped to <c>JwtRegisteredClaimNames.Sub</c>.
    /// </summary>
    public const string SubClaimType = "sub";

    public const int EnumColumnLength = 20;
}
