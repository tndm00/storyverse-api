namespace Library.Domain.Entities;

/// <summary>
/// A reader deliberately saving a story to their personal library ("tủ truyện").
/// This is an explicit follow/bookmark action — never inferred from reading
/// activity, which is tracked separately by <see cref="ReadingProgress"/>
/// (domain spec rule 07). Ownership is on <see cref="UserId"/>, sourced only
/// from the caller's validated JWT <c>sub</c> claim.
/// </summary>
public sealed class LibraryEntry : BaseEntity
{
    /// <summary>Stable public identifier exposed by the API instead of <see cref="BaseEntity.Id"/>.</summary>
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Owning user id, from the JWT <c>sub</c> claim. The User row lives in the
    /// Authentication service, so this is a plain value with no cross-service
    /// foreign key.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Referenced story, by its Content-service <c>PublicId</c>. No cross-service
    /// foreign key: the Story row is owned by the Content service.
    /// </summary>
    public Guid StoryId { get; set; }

    public ShelfStatus ShelfStatus { get; set; } = ShelfStatus.Reading;

    /// <summary>When the reader first added this story to their library.</summary>
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
}
