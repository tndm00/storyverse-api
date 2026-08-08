namespace Community.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string ConnectionStringName = "CommunityDatabase";

    /// <summary>
    /// Business-boundary database schema for community tables, per
    /// code-standard.md section 21 (Database Schema Rules).
    /// </summary>
    public const string CommunitySchemaName = "community";
}
