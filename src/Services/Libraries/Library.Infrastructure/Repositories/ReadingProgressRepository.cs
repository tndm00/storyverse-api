namespace Library.Infrastructure.Repositories;

public sealed class ReadingProgressRepository : IReadingProgressRepository
{
    private readonly LibraryDbContext _dbContext;

    public ReadingProgressRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ReadingProgress> GetAsync(long userId, Guid storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ReadingProgress
            .FirstOrDefaultAsync(x => x.UserId == userId && x.StoryId == storyId, cancellationToken);
    }

    public async Task<(IReadOnlyList<ReadingProgress> Items, int TotalCount)> GetRecentAsync(
        long userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ReadingProgress
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.LastReadAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(ReadingProgress progress, CancellationToken cancellationToken = default)
    {
        await _dbContext.ReadingProgress.AddAsync(progress, cancellationToken);
    }

    public void Update(ReadingProgress progress)
    {
        _dbContext.ReadingProgress.Update(progress);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
