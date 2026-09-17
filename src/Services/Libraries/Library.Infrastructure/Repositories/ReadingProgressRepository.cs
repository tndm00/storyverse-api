namespace Library.Infrastructure.Repositories;

/// <summary>EF Core-backed repository for <see cref="ReadingProgress"/> rows, scoped per request via the shared DbContext.</summary>
public sealed class ReadingProgressRepository : IReadingProgressRepository
{
    private readonly LibraryDbContext _dbContext;

    public ReadingProgressRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Fetches a reader's progress row for a story, tracked, or null if reading has not started.</summary>
    public Task<ReadingProgress> GetAsync(long userId, Guid storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.ReadingProgress
            .FirstOrDefaultAsync(x => x.UserId == userId && x.StoryId == storyId, cancellationToken);
    }

    /// <summary>Returns a page of a reader's reading progress, most-recently-read first, for the "continue reading" list.</summary>
    public async Task<(IReadOnlyList<ReadingProgress> Items, int TotalCount)> GetRecentAsync(
        long userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ReadingProgress
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        // Total count before paging, for the caller's PagedResponseDto.
        var totalCount = await query.CountAsync(cancellationToken);

        // Most-recently-read first, with a stable tiebreaker for equal timestamps.
        var items = await query
            .OrderByDescending(x => x.LastReadAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>Stages a new reading progress row for insertion; not persisted until <see cref="SaveChangesAsync"/>.</summary>
    public async Task AddAsync(ReadingProgress progress, CancellationToken cancellationToken = default)
    {
        await _dbContext.ReadingProgress.AddAsync(progress, cancellationToken);
    }

    /// <summary>Marks a reading progress row as modified; not persisted until <see cref="SaveChangesAsync"/>.</summary>
    public void Update(ReadingProgress progress)
    {
        _dbContext.ReadingProgress.Update(progress);
    }

    /// <summary>Persists all pending changes tracked by the DbContext.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
