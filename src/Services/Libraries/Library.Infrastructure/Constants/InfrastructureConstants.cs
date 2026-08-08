namespace Library.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string ConnectionStringName = "LibraryDatabase";

    /// <summary>
    /// Business-boundary database schema for library tables, per
    /// code-standard.md section 21 (Database Schema Rules).
    /// </summary>
    public const string LibrarySchemaName = "library";
}
