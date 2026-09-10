namespace Content.Api.Constants;

/// <summary>
/// Centralized route segments, per code-standard.md section 11 (Constants Rules)
/// and api-guidelines.md sections 3-4 (Versioning, URI Design).
/// </summary>
public static class ControllerRouteConstants
{
    public const string ApiVersion1 = "v1";

    public const string StoriesBase = "v1/stories";
    public const string ChaptersBase = "v1/chapters";
    public const string VolumesBase = "v1/volumes";
    public const string GenresBase = "v1/genres";
    public const string TagsBase = "v1/tags";

    public const string StoryQuickPublishSegment = "quick-publish";
    public const string StoryGuestPublishSegment = "guest-publish";

    // Admin/author listings (literal segments; matched ahead of "{storyId:guid}")
    public const string StoryAdminSegment = "admin";
    public const string StoryAdminCountsSegment = "admin/counts";
    public const string StoryMineSegment = "mine";

    public const string StoryByIdSegment = "{storyId:guid}";
    public const string StoryBySlugSegment = "by-slug/{slug}";
    public const string StoryGenresSegment = "{storyId:guid}/genres";
    public const string StoryTagsSegment = "{storyId:guid}/tags";
    public const string StoryStatusSegment = "{storyId:guid}/status";
    public const string StoryVolumesSegment = "{storyId:guid}/volumes";
    public const string StoryChaptersSegment = "{storyId:guid}/chapters";

    public const string VolumeByIdSegment = "{volumeId:guid}";

    public const string ChapterByIdSegment = "{chapterId:guid}";
    public const string ChapterSubmitForReviewSegment = "{chapterId:guid}/submit-for-review";
    public const string ChapterScheduleSegment = "{chapterId:guid}/schedule";
    public const string ChapterCancelScheduleSegment = "{chapterId:guid}/cancel-schedule";
    public const string ChapterRemoveSegment = "{chapterId:guid}/remove";

    // Moderation (content.moderate)
    public const string ChapterPendingReviewSegment = "pending-review";
    public const string ChapterReviewedSegment = "reviewed";
    public const string ChapterReviewCountsSegment = "review-counts";
    public const string ChapterForReviewSegment = "{chapterId:guid}/for-review";
    public const string ChapterReviewSegment = "{chapterId:guid}/review";
    public const string ChapterApproveSegment = "{chapterId:guid}/approve";
    public const string ChapterRejectSegment = "{chapterId:guid}/reject";

    public const string GenreBySlugSegment = "{slug}";
    public const string GenreHideSegment = "{slug}/hide";

    // Internal, service-to-service (X-Service-Token). Base is bare "v1" so each
    // route below carries its full path and never collides with the guid-id routes.
    public const string InternalBase = "v1";
    public const string InternalStoryModerationVisibilitySegment = "stories/{storyId:guid}/moderation-visibility";
    public const string InternalChapterModerationVisibilitySegment = "chapters/{chapterId:guid}/moderation-visibility";
    public const string InternalStoryTitlesSegment = "stories/internal/titles";
    public const string InternalChapterTitlesSegment = "chapters/internal/titles";
}
