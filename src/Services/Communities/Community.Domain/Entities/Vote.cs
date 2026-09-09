namespace Community.Domain.Entities;

/// <summary>
/// A weekly recommendation vote for a story, feeding the weekly ranking. One row
/// per (story, user, ISO week); casting again in the same week is a no-op. Kept
/// deliberately separate from any monetization concept.
/// </summary>
public sealed class Vote : BaseEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>Public id of the story being voted for (Content service).</summary>
    public Guid StoryId { get; set; }

    /// <summary>Voter's user id, taken from the validated JWT <c>sub</c> claim.</summary>
    public long UserId { get; set; }

    /// <summary>ISO-8601 week key, e.g. <c>2026-W32</c>.</summary>
    public string WeekKey { get; set; } = string.Empty;
}
