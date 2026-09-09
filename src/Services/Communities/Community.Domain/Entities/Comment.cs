namespace Community.Domain.Entities;

/// <summary>
/// A reader comment on a chapter. Threaded one level (or more) via the nullable
/// self-reference <see cref="ParentCommentId"/>. The chapter is owned by the
/// Content service, so it is referenced by its public id only — there is no
/// cross-service foreign key.
/// </summary>
public sealed class Comment : BaseEntity
{
    public Guid PublicId { get; set; } = Guid.NewGuid();

    /// <summary>Public id of the chapter this comment belongs to (Content service).</summary>
    public Guid ChapterId { get; set; }

    /// <summary>Author's user id, taken from the validated JWT <c>sub</c> claim.</summary>
    public long AuthorUserId { get; set; }

    /// <summary>
    /// Public id of the parent comment when this is a reply; null for a top-level
    /// comment. Modelled as a bare self-reference id (indexed, no database FK
    /// constraint), matching how the codebase references entities across
    /// aggregate boundaries.
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    public string Content { get; set; } = string.Empty;

    public CommentStatus Status { get; set; } = CommentStatus.Visible;

    /// <summary>Denormalized like count, maintained by the reaction flow (not Phase 1).</summary>
    public int LikeCount { get; set; }
}
