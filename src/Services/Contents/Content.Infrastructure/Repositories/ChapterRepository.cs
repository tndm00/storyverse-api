namespace Content.Infrastructure.Repositories;

public sealed class ChapterRepository : IChapterRepository
{
    private readonly ContentDbContext _dbContext;

    public ChapterRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Chapter> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chapters.FirstOrDefaultAsync(x => x.PublicId == publicId, cancellationToken);
    }

    public async Task<IReadOnlyList<Chapter>> GetByStoryAsync(
        long storyId,
        bool publishedOnly,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Chapters
            .AsNoTracking()
            .Where(x => x.StoryId == storyId);

        query = publishedOnly
            ? query.Where(x => x.Status == ChapterStatus.Published)
            : query.Where(x => x.Status != ChapterStatus.Removed);

        return await query.OrderBy(x => x.OrderIndex).ToListAsync(cancellationToken);
    }

    public Task<bool> StoryHasPublishedChapterAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chapters.AnyAsync(
            x => x.StoryId == storyId && x.Status == ChapterStatus.Published,
            cancellationToken);
    }

    public Task<decimal?> GetMaxOrderIndexAsync(long storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Chapters
            .Where(x => x.StoryId == storyId && x.Status != ChapterStatus.Removed)
            .MaxAsync(x => (decimal?)x.OrderIndex, cancellationToken);
    }

    public async Task AddAsync(Chapter chapter, CancellationToken cancellationToken = default)
    {
        await _dbContext.Chapters.AddAsync(chapter, cancellationToken);
    }

    public void Update(Chapter chapter)
    {
        _dbContext.Chapters.Update(chapter);
    }

    public async Task IncrementViewCountAsync(long chapterId, long storyId, CancellationToken cancellationToken = default)
    {
        await _dbContext.Chapters
            .Where(x => x.Id == chapterId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1), cancellationToken);

        await _dbContext.Stories
            .Where(x => x.Id == storyId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.ViewCount, x => x.ViewCount + 1), cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
