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

/// <summary>New chapter order within a volume or a story: every chapter id, in the desired sequence.</summary>
public sealed class ReorderChaptersRequestDto
{
    public IReadOnlyList<Guid> OrderedChapterIds { get; init; } = Array.Empty<Guid>();
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

/// <summary>
/// Chapter counts for the moderation review dashboard
/// (GET /v1/chapters/review-counts). Approved/Rejected are counted from the
/// review audit trail, so they survive later status changes.
/// </summary>
public sealed class ChapterReviewCountsResponseDto
{
    /// <summary>Chapters currently <c>PendingReview</c> (submitted, not yet picked up).</summary>
    public int Pending { get; init; }

    /// <summary>Chapters currently <c>InReview</c> (a moderator is deciding).</summary>
    public int InReview { get; init; }

    /// <summary>Distinct chapters that have ever been approved by a moderator.</summary>
    public int Approved { get; init; }

    /// <summary>Distinct chapters that have ever been rejected by a moderator.</summary>
    public int Rejected { get; init; }
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

    /// <summary>
    /// Chapter review timeline (who picked it up / approved / rejected, when, why),
    /// oldest first. Populated only by the moderator "for review" endpoint; empty
    /// on every other response.
    /// </summary>
    public IReadOnlyList<ChapterReviewActionResponseDto> ReviewActions { get; init; } =
        Array.Empty<ChapterReviewActionResponseDto>();
}

/// <summary>One entry of a chapter's review timeline.</summary>
public sealed class ChapterReviewActionResponseDto
{
    public Guid Id { get; init; }

    public long ModeratorUserId { get; init; }

    public string Action { get; init; }

    public string Note { get; init; }

    public DateTime CreatedAt { get; init; }
}
