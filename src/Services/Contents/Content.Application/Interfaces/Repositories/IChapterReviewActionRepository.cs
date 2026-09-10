namespace Content.Application.Interfaces.Repositories;

/// <summary>
/// Append-only persistence boundary for the <see cref="ChapterReviewAction"/>
/// audit trail. There is deliberately no update or delete member — audit records
/// are immutable.
/// </summary>
public interface IChapterReviewActionRepository
{
    Task AddAsync(ChapterReviewAction action, CancellationToken cancellationToken = default);

    /// <summary>Review timeline for a chapter, oldest first.</summary>
    Task<IReadOnlyList<ChapterReviewAction>> GetByChapterIdAsync(long chapterId, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
