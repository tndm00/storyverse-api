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

    public async Task<(IReadOnlyList<(Chapter Chapter, Story Story)> Items, int TotalCount)> GetPendingReviewAsync(
        ChapterStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var statuses = status is { } s
            ? new[] { s }
            : new[] { ChapterStatus.PendingReview, ChapterStatus.InReview };

        var query =
            from chapter in _dbContext.Chapters.AsNoTracking()
            join story in _dbContext.Stories.AsNoTracking() on chapter.StoryId equals story.Id
            where statuses.Contains(chapter.Status)
            orderby chapter.CreatedAt
            select new { chapter, story };

        var totalCount = await query.CountAsync(cancellationToken);

        var page = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        var items = page.Select(x => (x.chapter, x.story)).ToArray();
        return (items, totalCount);
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
