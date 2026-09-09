namespace Authentication.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string ConnectionStringName = "AuthenticationDatabase";

    /// <summary>
    /// Business-boundary database schema for identity tables, per
    /// code-standard.md section 21 (Database Schema Rules).
    /// </summary>
    public const string IdentitySchemaName = "identity";

    public const string UsersTableName = "users";

    public const string AuthorProfilesTableName = "author_profiles";

    public const string UserRolesTableName = "user_roles";

    /// <summary>
    /// JWT claim type for the subject, used as a fallback when the inbound
    /// claim was not remapped to <c>JwtRegisteredClaimNames.Sub</c>.
    /// </summary>
    public const string SubClaimType = "sub";
}
