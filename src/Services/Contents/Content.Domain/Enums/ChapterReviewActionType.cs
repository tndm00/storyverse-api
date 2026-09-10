namespace Content.Domain.Enums;

/// <summary>
/// The kind of moderator decision captured by a <see cref="Content.Domain.Entities.ChapterReviewAction"/>
/// audit row.
/// </summary>
public enum ChapterReviewActionType
{
    /// <summary>A moderator picked the chapter up for review (PendingReview to InReview).</summary>
    Reviewed,

    /// <summary>A moderator approved and published the chapter.</summary>
    Approved,

    /// <summary>A moderator rejected the chapter.</summary>
    Rejected
}
