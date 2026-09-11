namespace Community.Infrastructure.Repositories;

public sealed class RatingRepository : IRatingRepository
{
    private readonly CommunityDbContext _dbContext;

    public RatingRepository(CommunityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Rating> GetByStoryAndUserAsync(Guid storyId, long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Ratings.FirstOrDefaultAsync(
            x => x.StoryId == storyId && x.UserId == userId, cancellationToken);
    }

    public async Task<(IReadOnlyList<Rating> Items, int TotalCount)> GetByStoryAsync(
        Guid storyId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Ratings
            .AsNoTracking()
            .Where(x => x.StoryId == storyId);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(decimal AverageScore, int RatingCount)> GetAggregateByStoryAsync(
        Guid storyId, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Ratings.AsNoTracking().Where(x => x.StoryId == storyId);

        var count = await query.CountAsync(cancellationToken);
        if (count == 0)
        {
            return (0m, 0);
        }

        var average = await query.AverageAsync(x => (decimal)x.Score, cancellationToken);
        return (average, count);
    }

    public async Task AddAsync(Rating rating, CancellationToken cancellationToken = default)
    {
        await _dbContext.Ratings.AddAsync(rating, cancellationToken);
    }

    public void Update(Rating rating)
    {
        _dbContext.Ratings.Update(rating);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
