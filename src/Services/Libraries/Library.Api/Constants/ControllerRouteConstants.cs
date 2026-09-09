namespace Library.Api.Constants;

/// <summary>
/// Centralized route segments, per code-standard.md section 11 (Constants Rules)
/// and api-guidelines.md sections 3-4 (Versioning, URI Design).
/// </summary>
public static class ControllerRouteConstants
{
    public const string ApiVersion1 = "v1";

    public const string LibraryBase = "v1/library";
    public const string ReadingProgressBase = "v1/reading-progress";

    public const string LibraryEntryByStorySegment = "{storyId:guid}";
    public const string LibraryEntryShelfStatusSegment = "{storyId:guid}/shelf-status";

    public const string ReadingProgressContinueSegment = "continue-reading";
    public const string ReadingProgressByStorySegment = "{storyId:guid}";
}
