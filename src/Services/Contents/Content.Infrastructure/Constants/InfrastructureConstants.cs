namespace Content.Infrastructure.Constants;

public static class InfrastructureConstants
{
    public const string ConnectionStringName = "ContentDatabase";

    /// <summary>
    /// Business-boundary database schema for content tables, per
    /// code-standard.md section 21 (Database Schema Rules).
    /// </summary>
    public const string ContentSchemaName = "content";

    public const string StoriesTableName = "stories";
    public const string VolumesTableName = "volumes";
    public const string ChaptersTableName = "chapters";
    public const string GenresTableName = "genres";
    public const string TagsTableName = "tags";
    public const string StoryGenresTableName = "story_genres";
    public const string StoryTagsTableName = "story_tags";

    /// <summary>
    /// Fallback JWT subject claim name, used when the inbound claim was not
    /// remapped to <see cref="JwtRegisteredClaimNames.Sub"/>.
    /// </summary>
    public const string SubClaimType = "sub";

    public const int EnumColumnLength = 20;
}
