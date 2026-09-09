namespace Library.Application.Constants;

/// <summary>
/// Centralized business error messages surfaced through the standard error
/// envelope, per code-standard.md section 11.
/// </summary>
public static class ApplicationErrorConstants
{
    public const string CallerNotAuthenticated = "The request is not authenticated.";

    public const string LibraryEntryNotFound = "This story is not in your library.";
    public const string LibraryEntryAlreadyExists = "This story is already in your library.";
    public const string ReadingProgressNotFound = "No reading progress recorded for this story.";

    public const string StoryIdRequired = "A story id is required.";
    public const string ChapterIdRequired = "A chapter id is required.";
    public const string InvalidShelfStatus = "That shelf status is not recognized.";
    public const string InvalidScrollPercent = "Scroll percent must be between 0 and 100.";
    public const string InvalidPageParameters = "Invalid pagination parameters.";
}
