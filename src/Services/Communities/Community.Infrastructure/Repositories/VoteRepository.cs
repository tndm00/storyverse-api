namespace Community.Infrastructure.Repositories;

/// <summary>EF Core-backed repository for <see cref="Vote"/> reads and writes.</summary>
public sealed class VoteRepository : IVoteRepository
{
    private readonly CommunityDbContext _dbContext;

    /// <summary>Creates the repository over the scoped <see cref="CommunityDbContext"/>.</summary>
    public VoteRepository(CommunityDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Checks whether the user has already voted for the story in the given ISO week.</summary>
    public Task<bool> ExistsAsync(Guid storyId, long userId, string weekKey, CancellationToken cancellationToken = default)
    {
        return _dbContext.Votes.AnyAsync(
            x => x.StoryId == storyId && x.UserId == userId && x.WeekKey == weekKey, cancellationToken);
    }

    /// <summary>Counts the votes cast for a story in a given ISO week (the public tally).</summary>
    public async Task<int> CountForStoryWeekAsync(
        Guid storyId,
        string weekKey,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Votes
            .AsNoTracking()
            .CountAsync(x => x.StoryId == storyId && x.WeekKey == weekKey, cancellationToken);
    }

    /// <summary>Stages a new vote for insertion; not persisted until <see cref="SaveChangesAsync"/>.</summary>
    public async Task AddAsync(Vote vote, CancellationToken cancellationToken = default)
    {
        await _dbContext.Votes.AddAsync(vote, cancellationToken);
    }

    /// <summary>Persists all pending changes to the database.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
