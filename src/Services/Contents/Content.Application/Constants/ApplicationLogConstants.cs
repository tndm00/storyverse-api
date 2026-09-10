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
    public const string ChapterSubmittedForReview = "Chapter {ChapterId} submitted for review (story {StoryId}).";
    public const string ChapterReviewStarted = "Chapter {ChapterId} review started by moderator {ModeratorUserId}.";
    public const string ChapterApproved = "Chapter {ChapterId} approved and published for story {StoryId}.";
    public const string ChapterRejected = "Chapter {ChapterId} rejected by moderator {ModeratorUserId}.";
    public const string ChapterScheduled = "Chapter {ChapterId} scheduled for {ScheduledAt}.";
    public const string ScheduledChapterAutoPublished =
        "Scheduled chapter {ChapterId} auto-published for story {StoryId} (was due at {ScheduledAt}).";
    public const string ScheduledChapterPublisherCompleted =
        "Scheduled chapter publisher pass complete: published {PublishedCount} of {DueCount} due chapter(s).";
    public const string ScheduledChapterPublisherDisabled =
        "Scheduled chapter publisher is disabled by configuration (ChapterPublishing:Enabled=false); not starting.";
    public const string ScheduledChapterPublisherStarting =
        "Scheduled chapter publisher started; polling every {PollIntervalSeconds}s.";
    public const string ScheduledChapterPublisherFailed =
        "Scheduled chapter publisher pass failed; will retry on the next interval.";
    public const string ChapterScheduleCancelled = "Chapter {ChapterId} schedule cancelled.";
    public const string ChapterRemoved = "Chapter {ChapterId} removed.";
    public const string StoryModerationVisibilityChanged = "Story {StoryId} moderation visibility changed (hidden={Hidden}); status now {Status}.";
    public const string ChapterModerationVisibilityChanged = "Chapter {ChapterId} moderation visibility changed (hidden={Hidden}); status now {Status}.";

    public const string GenreCreated = "Genre {GenreId} created.";
    public const string GenreUpdated = "Genre {GenreId} updated.";
    public const string GenreHidden = "Genre {GenreId} hidden.";

    public const string OwnershipCheckFailed = "Author {AuthorProfileId} attempted to modify story {StoryId} they do not own.";

    public const string ChapterReviewNotificationFailed = "Failed to send {NotificationKind} notification for chapter {ChapterId} (author profile {AuthorProfileId}); approve/reject was not affected.";
    public const string ChapterReviewNotificationSkippedGuest = "Chapter {ChapterId} belongs to a guest author; skipped {NotificationKind} notification.";
    public const string ChapterReviewNotificationRecipientUnresolved = "Could not resolve an author user id for chapter {ChapterId} (author profile {AuthorProfileId}); skipped {NotificationKind} notification.";
}
