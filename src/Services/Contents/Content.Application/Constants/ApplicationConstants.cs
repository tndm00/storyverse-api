namespace Content.Application.Constants;

/// <summary>
/// Content service application-layer values: business labels and limits that are
/// not routes, log templates, or error messages.
/// </summary>
public static class ApplicationConstants
{
    public const string DefaultLanguage = "vi";

    public const int MinPageSize = 1;
    public const int MaxPageSize = 50;
    public const int DefaultPageSize = 20;

    public const int PopularTagsCount = 30;

    public const int MaxTitleLength = 200;
    public const int MaxSlugLength = 220;
    public const int MaxDescriptionLength = 5000;
    public const int MaxUrlLength = 2048;
    public const int MaxTagNameLength = 60;
    public const int MaxGenreNameLength = 60;
    public const int MaxChapterTitleLength = 200;
    public const int MaxTagsPerStory = 30;

    /// <summary>Separator appended with a counter when a generated slug already exists.</summary>
    public const string SlugCollisionSeparator = "-";

    /// <summary>
    /// Spacing between auto-assigned chapter order indexes. Whole-number spacing
    /// leaves room to insert a chapter between two others with a fractional value.
    /// </summary>
    public const decimal ChapterOrderIndexGap = 1m;

    /// <summary>Spacing between auto-assigned volume order indexes.</summary>
    public const int VolumeOrderIndexGap = 1;

    /// <summary>How many top stories the admin view statistics list.</summary>
    public const int ViewStatsTopStoriesCount = 5;
}
