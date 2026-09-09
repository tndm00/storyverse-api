namespace Community.Application.Interfaces.Repositories;

/// <summary>Persistence boundary for per-story <see cref="Rating"/> rows.</summary>
public interface IRatingRepository
{
    Task<Rating> GetByStoryAndUserAsync(Guid storyId, long userId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Rating> Items, int TotalCount)> GetByStoryAsync(
        Guid storyId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(Rating rating, CancellationToken cancellationToken = default);

    void Update(Rating rating);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
