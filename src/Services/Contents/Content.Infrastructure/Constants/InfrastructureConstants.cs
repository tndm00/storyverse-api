namespace Content.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string ConnectionStringName = "ContentDatabase";

    /// <summary>
    /// Business-boundary database schema for content tables, per
    /// code-standard.md section 21 (Database Schema Rules).
    /// </summary>
    public const string ContentSchemaName = "content";
}
