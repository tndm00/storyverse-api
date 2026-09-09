namespace Content.Application.Dtos;

/// <summary>Payload to create a chapter (author only).</summary>
public sealed class CreateChapterRequestDto
{
    public string Title { get; init; } = string.Empty;

    /// <summary><c>0</c> (default) appends after the last chapter; a fractional value inserts between two.</summary>
    public decimal OrderIndex { get; init; }

    public string Content { get; init; } = string.Empty;

    public Guid? VolumeId { get; init; }

    /// <summary>Publish the chapter in the same request instead of leaving it as a draft.</summary>
    public bool PublishImmediately { get; init; }
}

/// <summary>Editable chapter fields.</summary>
public sealed class UpdateChapterRequestDto
{
    public string Title { get; init; } = string.Empty;

    public decimal OrderIndex { get; init; }

    public string Content { get; init; } = string.Empty;

    public Guid? VolumeId { get; init; }
}

/// <summary>Schedules a draft chapter to publish automatically at a future time.</summary>
public sealed class ScheduleChapterRequestDto
{
    public DateTime ScheduledAt { get; init; }
}

/// <summary>Moderator's decision when rejecting a chapter in review.</summary>
public sealed class RejectChapterRequestDto
{
    public string Reason { get; init; } = string.Empty;
}

/// <summary>Chapter entry in a story's table of contents.</summary>
public sealed class ChapterSummaryResponseDto
{
    public Guid Id { get; init; }

    public Guid? VolumeId { get; init; }

    public string Title { get; init; }

    public decimal OrderIndex { get; init; }

    public int WordCount { get; init; }

    public string Status { get; init; }

    public int ViewCount { get; init; }

    public int CommentCount { get; init; }

    public DateTime? PublishedAt { get; init; }

    /// <summary>Moderator's note when <c>Status</c> is <c>Rejected</c>; null otherwise.</summary>
    public string RejectionReason { get; init; }
}

/// <summary>One row of the cross-story moderation queue (GET /v1/chapters/pending-review).</summary>
public sealed class PendingReviewChapterResponseDto
{
    public Guid ChapterId { get; init; }

    public Guid StoryId { get; init; }

    public string StorySlug { get; init; }

    public string StoryTitle { get; init; }

    public string ChapterTitle { get; init; }

    public int WordCount { get; init; }

    public long AuthorProfileId { get; init; }

    public string GuestAuthorName { get; init; }

    public string Status { get; init; }

    public DateTime CreatedAt { get; init; }
}

/// <summary>Full chapter content for the reader view.</summary>
public sealed class ChapterDetailResponseDto
{
    public Guid Id { get; init; }

    public Guid StoryId { get; init; }

    public Guid? VolumeId { get; init; }

    public string Title { get; init; }

    public decimal OrderIndex { get; init; }

    public string Content { get; init; }

    public int WordCount { get; init; }

    public string Status { get; init; }

    public int ViewCount { get; init; }

    public int CommentCount { get; init; }

    public DateTime? ScheduledAt { get; init; }

    public DateTime? PublishedAt { get; init; }

    public DateTime CreatedAt { get; init; }

    /// <summary>Moderator's note when <c>Status</c> is <c>Rejected</c>; null otherwise.</summary>
    public string RejectionReason { get; init; }
}
