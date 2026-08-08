namespace Moderation.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string ConnectionStringName = "ModerationDatabase";

    /// <summary>
    /// Business-boundary database schema for moderation tables, per
    /// code-standard.md section 21 (Database Schema Rules).
    /// </summary>
    public const string ModerationSchemaName = "moderation";
}
