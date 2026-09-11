namespace Content.Application.Interfaces.Repositories;

/// <summary>
/// Append-only persistence boundary for the <see cref="ChapterReviewAction"/>
/// audit trail. There is deliberately no update or delete member — audit records
/// are immutable at the application layer. Note, however, that an audit row
/// will be cascade-deleted at the database level along with its owning
/// Chapter if the owning Story is hard-deleted; that only ever happens while
/// the Story is still in Draft status (never published/public) — see
/// <c>DeleteStoryCommandHandler</c>.
/// </summary>
public interface IChapterReviewActionRepository
{
    Task AddAsync(ChapterReviewAction action, CancellationToken cancellationToken = default);

    /// <summary>Review timeline for a chapter, oldest first.</summary>
    Task<IReadOnlyList<ChapterReviewAction>> GetByChapterIdAsync(long chapterId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
