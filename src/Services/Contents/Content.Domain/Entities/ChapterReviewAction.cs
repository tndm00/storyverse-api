namespace Content.Domain.Entities;

/// <summary>
/// Immutable audit record of a moderator decision on a chapter review (who
/// picked it up / approved / rejected, when, and why). Written once and NEVER
/// updated or deleted — this is the chapter review timeline shown to the FE.
/// Mirrors the Moderation service's <c>ModerationAction</c> pattern.
/// </summary>
public sealed class ChapterReviewAction : BaseEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>Owning chapter row id (content.chapters.Id).</summary>
    public long ChapterId { get; set; }

    /// <summary>User id of the acting moderator, taken from the JWT <c>sub</c> claim.</summary>
    public long ModeratorUserId { get; set; }

    public ChapterReviewActionType Action { get; set; }

    /// <summary>Optional note; the rejection reason for a <see cref="ChapterReviewActionType.Rejected"/> row.</summary>
    public string Note { get; set; }
}
