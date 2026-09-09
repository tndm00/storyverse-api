namespace Content.Domain.Enums;

/// <summary>
/// Lifecycle of a single <see cref="Content.Domain.Entities.Chapter"/>, per
/// product-workflow-context.md section 7. A chapter has its own lifecycle
/// independent of its parent story's <see cref="StoryStatus"/>.
/// </summary>
public enum ChapterStatus
{
    Draft,
    Scheduled,

    /// <summary>Submitted by the author, waiting for a moderator to pick it up.</summary>
    PendingReview,

    /// <summary>A moderator has picked it up and is deciding.</summary>
    InReview,

    Published,

    /// <summary>Reviewed and rejected; the author may edit and resubmit for review.</summary>
    Rejected,

    Removed
}
