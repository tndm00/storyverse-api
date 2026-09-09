namespace Community.Domain.Entities;

/// <summary>
/// A reader's star rating and optional review for a story. Exactly one row per
/// (story, user) — a second submission updates the existing row. The story is
/// owned by the Content service and referenced by its public id only.
/// </summary>
public sealed class Rating : BaseEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>Public id of the story being rated (Content service).</summary>
    public Guid StoryId { get; set; }

    /// <summary>Rater's user id, taken from the validated JWT <c>sub</c> claim.</summary>
    public long UserId { get; set; }

    /// <summary>Score from 1 to 5.</summary>
    public int Score { get; set; }

    public string ReviewText { get; set; }
}
