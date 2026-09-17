namespace Content.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IChapterReviewActionRepository"/>, the
/// append-only audit trail of moderator actions on chapters.
/// </summary>
public sealed class ChapterReviewActionRepository : IChapterReviewActionRepository
{
    private readonly ContentDbContext _dbContext;

    public ChapterReviewActionRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Queues a new review action for insertion; call <see cref="SaveChangesAsync"/> to persist.</summary>
    public async Task AddAsync(ChapterReviewAction action, CancellationToken cancellationToken = default)
    {
        await _dbContext.ChapterReviewActions.AddAsync(action, cancellationToken);
    }

    /// <summary>Lists a chapter's full review history, oldest first.</summary>
    public async Task<IReadOnlyList<ChapterReviewAction>> GetByChapterIdAsync(
        long chapterId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.ChapterReviewActions
            .AsNoTracking()
            .Where(x => x.ChapterId == chapterId)
            .OrderBy(x => x.CreatedAt)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Persists all pending changes tracked by the context.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
