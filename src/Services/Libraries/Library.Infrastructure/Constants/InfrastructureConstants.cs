namespace Library.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string ConnectionStringName = "LibraryDatabase";

    /// <summary>
    /// Business-boundary database schema for library tables, per
    /// code-standard.md section 21 (Database Schema Rules).
    /// </summary>
    public const string LibrarySchemaName = "library";

    public const string LibraryEntriesTableName = "library_entries";
    public const string ReadingProgressTableName = "reading_progress";

    /// <summary>
    /// Fallback JWT subject claim name, used when the inbound claim was not
    /// remapped to <see cref="System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub"/>.
    /// </summary>
    public const string SubClaimType = "sub";

    public const int EnumColumnLength = 20;
}
