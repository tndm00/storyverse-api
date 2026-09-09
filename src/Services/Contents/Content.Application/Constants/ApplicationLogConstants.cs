namespace Content.Application.Constants;

/// <summary>
/// Structured-logging message templates for the Content service. No secrets or
/// full chapter content in these templates, per code-standard.md section 12.
/// </summary>
public static class ApplicationLogConstants
{
    public const string StoryCreated = "Story {StoryId} created by author {AuthorProfileId}.";
    public const string StoryUpdated = "Story {StoryId} updated.";
    public const string StoryGenresAssigned = "Story {StoryId} genres reassigned ({GenreCount} genres).";
    public const string StoryTagsAssigned = "Story {StoryId} tags reassigned ({TagCount} tags).";
    public const string StoryStatusChanged = "Story {StoryId} status changed from {FromStatus} to {ToStatus}.";
    public const string StoryAutoOngoing = "Story {StoryId} auto-transitioned to Ongoing on first chapter publish.";

    public const string StoryQuickPublished = "Story {StoryId} quick-published by author {AuthorProfileId} (first chapter {ChapterId}, completed {Completed}).";

    public const string ChapterCreated = "Chapter {ChapterId} created for story {StoryId}.";
    public const string ChapterOrderAutoAssigned = "Chapter {ChapterId} order index auto-assigned to {OrderIndex} for story {StoryId}.";
    public const string ChapterUpdated = "Chapter {ChapterId} updated.";
    public const string ChapterEditedAfterPublish = "Chapter {ChapterId} was edited in place after publication; consider versioning for substantial rewrites.";
    public const string ChapterPublished = "Chapter {ChapterId} published for story {StoryId}.";
    public const string ChapterScheduled = "Chapter {ChapterId} scheduled for {ScheduledAt}.";
    public const string ChapterScheduleCancelled = "Chapter {ChapterId} schedule cancelled.";
    public const string ChapterRemoved = "Chapter {ChapterId} removed.";

    public const string GenreCreated = "Genre {GenreId} created.";
    public const string GenreUpdated = "Genre {GenreId} updated.";
    public const string GenreHidden = "Genre {GenreId} hidden.";

    public const string OwnershipCheckFailed = "Author {AuthorProfileId} attempted to modify story {StoryId} they do not own.";
}
