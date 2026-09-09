namespace Community.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string ConnectionStringName = "CommunityDatabase";

    /// <summary>
    /// Business-boundary database schema for community tables, per
    /// code-standard.md section 21 (Database Schema Rules).
    /// </summary>
    public const string CommunitySchemaName = "community";

    public const string CommentsTableName = "comments";
    public const string RatingsTableName = "ratings";
    public const string VotesTableName = "votes";

    /// <summary>
    /// Fallback JWT subject claim name, used when the inbound claim was not
    /// remapped to <see cref="JwtRegisteredClaimNames.Sub"/>.
    /// </summary>
    public const string SubClaimType = "sub";

    public const int EnumColumnLength = 20;

    public const int WeekKeyColumnLength = 10;
}
