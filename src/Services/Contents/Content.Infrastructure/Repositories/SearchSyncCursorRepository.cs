namespace Content.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="ISearchSyncCursorRepository"/>. Tracks a
/// single-row cursor (id 1) marking the last time content was synced to the search index.
/// </summary>
public sealed class SearchSyncCursorRepository : ISearchSyncCursorRepository
{
    private const long CursorRowId = 1;

    private readonly ContentDbContext _dbContext;

    public SearchSyncCursorRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Gets the last-synced timestamp, or <see cref="DateTime.MinValue"/> if sync has never run.</summary>
    public async Task<DateTime> GetLastSyncedAtAsync(CancellationToken cancellationToken = default)
    {
        var cursor = await _dbContext.SearchSyncCursors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == CursorRowId, cancellationToken);

        return cursor?.LastSyncedAt ?? DateTime.MinValue;
    }

    /// <summary>
    /// Upserts the single cursor row with the new last-synced timestamp, creating it on
    /// first sync and updating it thereafter.
    /// </summary>
    public async Task SetLastSyncedAtAsync(DateTime value, CancellationToken cancellationToken = default)
    {
        var cursor = await _dbContext.SearchSyncCursors
            .FirstOrDefaultAsync(x => x.Id == CursorRowId, cancellationToken);

        // First sync ever: create the row; otherwise update it in place.
        if (cursor is null)
        {
            _dbContext.SearchSyncCursors.Add(new SearchSyncCursor { Id = CursorRowId, LastSyncedAt = value });
        }
        else
        {
            cursor.LastSyncedAt = value;
            cursor.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
