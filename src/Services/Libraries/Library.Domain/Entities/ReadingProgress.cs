namespace Library.Domain.Entities;

/// <summary>
/// Automatic record of the last chapter a reader opened in a story, powering the
/// "đọc tiếp" (continue reading) experience. Written implicitly whenever a
/// chapter is opened — never a deliberate user action, unlike
/// <see cref="LibraryEntry"/> (domain spec rule 07). A free read produces no
/// revenue but is still tracked here for ranking and resume.
/// </summary>
public sealed class ReadingProgress : BaseEntity
{
    /// <summary>Stable public identifier exposed by the API instead of <see cref="BaseEntity.Id"/>.</summary>
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>Owning user id, from the JWT <c>sub</c> claim. No cross-service foreign key.</summary>
    public long UserId { get; set; }

    /// <summary>Referenced story, by its Content-service <c>PublicId</c>. No cross-service foreign key.</summary>
    public Guid StoryId { get; set; }

    /// <summary>Last chapter opened, by its Content-service <c>PublicId</c>. No cross-service foreign key.</summary>
    public Guid LastChapterId { get; set; }

    /// <summary>Optional scroll position within the chapter, for precise resume.</summary>
    public decimal? ScrollPercent { get; set; }

    public DateTime LastReadAt { get; set; } = DateTime.UtcNow;
}
