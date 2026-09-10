namespace Content.Infrastructure.Repositories;

public sealed class ChapterReviewActionRepository : IChapterReviewActionRepository
{
    private readonly ContentDbContext _dbContext;

    public ChapterReviewActionRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(ChapterReviewAction action, CancellationToken cancellationToken = default)
    {
        await _dbContext.ChapterReviewActions.AddAsync(action, cancellationToken);
    }

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

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
