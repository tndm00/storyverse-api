namespace Content.Domain.Entities;

/// <summary>
/// The real readable unit of a story. Has its own lifecycle
/// (<see cref="ChapterStatus"/>) independent of the parent story's status.
/// </summary>
public sealed class Chapter : BaseEntity
{
    public long StoryId { get; set; }

    /// <summary>Optional owning volume.</summary>
    public long? VolumeId { get; set; }

    public Guid PublicId { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Sort position within the story. Decimal so a chapter can be inserted
    /// between two existing chapters without renumbering.
    /// </summary>
    public decimal OrderIndex { get; set; }

    public string Content { get; set; } = string.Empty;

    /// <summary>Word count, recomputed from <see cref="Content"/> on every write.</summary>
    public int WordCount { get; set; }

    public ChapterStatus Status { get; set; } = ChapterStatus.Draft;

    /// <summary>Moderator's note when <see cref="Status"/> is <see cref="ChapterStatus.Rejected"/>; cleared on resubmit.</summary>
    public string RejectionReason { get; set; }

    /// <summary>Phase 2 field. Always <see cref="ChapterAccessType.Free"/> in Phase 1.</summary>
    public ChapterAccessType AccessType { get; set; } = ChapterAccessType.Free;

    /// <summary>Phase 2 field. Always null in Phase 1.</summary>
    public int? PriceCoin { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public DateTime? PublishedAt { get; set; }

    public int ViewCount { get; set; }

    /// <summary>Denormalized comment count. Maintained by the Community service (not Phase 1A).</summary>
    public int CommentCount { get; set; }
}
