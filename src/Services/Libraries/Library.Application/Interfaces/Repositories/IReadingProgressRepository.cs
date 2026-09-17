namespace Library.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="ReadingProgress"/>. All reads are scoped
/// to the calling user id, which comes only from the validated JWT.
/// </summary>
public interface IReadingProgressRepository
{
    /// <summary>Loads a user's reading progress for a single story, or null when none has been recorded.</summary>
    Task<ReadingProgress> GetAsync(long userId, Guid storyId, CancellationToken cancellationToken = default);

    /// <summary>Paged "continue reading" listing for a user, most recently read first.</summary>
    Task<(IReadOnlyList<ReadingProgress> Items, int TotalCount)> GetRecentAsync(
        long userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Registers a new reading progress row to be inserted on the next <see cref="SaveChangesAsync"/>.</summary>
    Task AddAsync(ReadingProgress progress, CancellationToken cancellationToken = default);

    /// <summary>Marks a tracked reading progress row as modified for the next <see cref="SaveChangesAsync"/>.</summary>
    void Update(ReadingProgress progress);

    /// <summary>Persists all pending changes tracked by this repository's unit of work.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
