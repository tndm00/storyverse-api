namespace Library.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="ReadingProgress"/>. All reads are scoped
/// to the calling user id, which comes only from the validated JWT.
/// </summary>
public interface IReadingProgressRepository
{
    Task<ReadingProgress> GetAsync(long userId, Guid storyId, CancellationToken cancellationToken = default);

    /// <summary>Paged "continue reading" listing for a user, most recently read first.</summary>
    Task<(IReadOnlyList<ReadingProgress> Items, int TotalCount)> GetRecentAsync(
        long userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(ReadingProgress progress, CancellationToken cancellationToken = default);

    void Update(ReadingProgress progress);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
