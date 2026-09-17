namespace Community.Infrastructure.Repositories;

/// <summary>EF Core-backed repository for <see cref="Rating"/> reads and writes.</summary>
public sealed class RatingRepository : IRatingRepository
{
    private readonly CommunityDbContext _dbContext;

    /// <summary>Creates the repository over the scoped <see cref="CommunityDbContext"/>.</summary>
    public RatingRepository(CommunityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Finds the caller's own rating for a story, tracked (for subsequent updates).</summary>
    public Task<Rating> GetByStoryAndUserAsync(Guid storyId, long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Ratings.FirstOrDefaultAsync(
            x => x.StoryId == storyId && x.UserId == userId, cancellationToken);
    }

    /// <summary>Paged, newest-first list of ratings for a story, plus the total matching count.</summary>
    public async Task<(IReadOnlyList<Rating> Items, int TotalCount)> GetByStoryAsync(
        Guid storyId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Ratings
            .AsNoTracking()
            .Where(x => x.StoryId == storyId);

        // Count first for pagination metadata, then fetch the requested page.
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>Computes the average score and count of ratings for a story; returns (0, 0) when there are none.</summary>
    public async Task<(decimal AverageScore, int RatingCount)> GetAggregateByStoryAsync(
        Guid storyId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Ratings.AsNoTracking().Where(x => x.StoryId == storyId);

        // Avoid dividing by zero: short-circuit when the story has no ratings yet.
        var count = await query.CountAsync(cancellationToken);
        if (count == 0)
        {
            return (0m, 0);
        }

        var average = await query.AverageAsync(x => (decimal)x.Score, cancellationToken);
        return (average, count);
    }

    /// <summary>Stages a new rating for insertion; not persisted until <see cref="SaveChangesAsync"/>.</summary>
    public async Task AddAsync(Rating rating, CancellationToken cancellationToken = default)
    {
        await _dbContext.Ratings.AddAsync(rating, cancellationToken);
    }

    /// <summary>Marks an already-tracked rating as modified.</summary>
    public void Update(Rating rating)
    {
        _dbContext.Ratings.Update(rating);
    }

    /// <summary>Persists all pending changes to the database.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
