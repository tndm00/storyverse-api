namespace Content.Application.Constants;

/// <summary>
/// Centralized business error messages surfaced through the standard error
/// envelope, per code-standard.md section 11.
/// </summary>
public static class ApplicationErrorConstants
{
    public const string CallerIsNotAuthor = "The current account does not have an author profile.";
    public const string NotStoryOwner = "You do not own this story.";

    public const string StoryNotFound = "Story not found.";
    public const string ChapterNotFound = "Chapter not found.";
    public const string VolumeNotFound = "Volume not found.";
    public const string GenreNotFound = "Genre not found.";

    public const string SlugAlreadyUsed = "A story with a similar title already exists.";
    public const string GenreNameAlreadyUsed = "A genre with this name already exists.";

    public const string PrimaryGenreRequired = "The story must have exactly one primary genre before a chapter can be published.";
    public const string ExactlyOnePrimaryGenreRequired = "Assign exactly one primary genre.";
    public const string GenreInactiveOrMissing = "One or more selected genres do not exist or are inactive.";
    public const string DuplicateGenreSelection = "The same genre was selected more than once.";
    public const string TooManyTags = "A story cannot have more than the allowed number of tags.";
    public const string DuplicateQuickPublish = "You already have a story with this title. Edit that story instead of publishing a new one.";

    public const string InvalidStoryStatusTransition = "That story status change is not allowed.";
    public const string InvalidChapterStatusTransition = "That chapter status change is not allowed.";
    public const string RejectionReasonRequired = "A reason is required to reject a chapter.";
    public const string ScheduledTimeMustBeFuture = "The scheduled publish time must be in the future.";
    public const string VolumeStoryMismatch = "The volume does not belong to this story.";

    public const string TitleRequired = "Title is required.";
    public const string ContentRequired = "Chapter content is required.";
    public const string OriginalSourceRequired = "Original source is required for a translated work.";
    public const string InvalidPageParameters = "Invalid pagination parameters.";
}
