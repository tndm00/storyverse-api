namespace Content.Application.Interfaces.Repositories;

/// <summary>
/// Persistence for the background search-index sync job's high-water mark.
/// See <see cref="Content.Domain.Entities.SearchSyncCursor"/>.
/// </summary>
public interface ISearchSyncCursorRepository
{
    /// <summary>
    /// <see cref="DateTime.MinValue"/> when no sync has ever run — the job then
    /// treats everything as "changed" up to its cutoff, effectively acting as a
    /// full backfill on its very first tick.
    /// </summary>
    Task<DateTime> GetLastSyncedAtAsync(CancellationToken cancellationToken = default);

    Task SetLastSyncedAtAsync(DateTime value, CancellationToken cancellationToken = default);
}
