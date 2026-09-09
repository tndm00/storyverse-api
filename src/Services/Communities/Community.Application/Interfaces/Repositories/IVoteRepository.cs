namespace Community.Application.Interfaces.Repositories;

/// <summary>Persistence boundary for weekly ranking <see cref="Vote"/> rows.</summary>
public interface IVoteRepository
{
    Task<bool> ExistsAsync(Guid storyId, long userId, string weekKey, CancellationToken cancellationToken = default);

    Task<int> CountForStoryWeekAsync(Guid storyId, string weekKey, CancellationToken cancellationToken = default);

    Task AddAsync(Vote vote, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
