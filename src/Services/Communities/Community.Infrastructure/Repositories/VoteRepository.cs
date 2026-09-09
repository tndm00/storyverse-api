namespace Community.Infrastructure.Repositories;

public sealed class VoteRepository : IVoteRepository
{
    private readonly CommunityDbContext _dbContext;

    public VoteRepository(CommunityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<bool> ExistsAsync(Guid storyId, long userId, string weekKey, CancellationToken cancellationToken = default)
    {
        return _dbContext.Votes.AnyAsync(
            x => x.StoryId == storyId && x.UserId == userId && x.WeekKey == weekKey, cancellationToken);
    }

    public async Task<int> CountForStoryWeekAsync(
        Guid storyId,
        string weekKey,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Votes
            .AsNoTracking()
            .CountAsync(x => x.StoryId == storyId && x.WeekKey == weekKey, cancellationToken);
    }

    public async Task AddAsync(Vote vote, CancellationToken cancellationToken = default)
    {
        await _dbContext.Votes.AddAsync(vote, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
