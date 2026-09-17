namespace Content.Infrastructure.Repositories;

public sealed class SearchSyncCursorRepository : ISearchSyncCursorRepository
{
    private const long CursorRowId = 1;

    private readonly ContentDbContext _dbContext;

    public SearchSyncCursorRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<DateTime> GetLastSyncedAtAsync(CancellationToken cancellationToken = default)
    {
        var cursor = await _dbContext.SearchSyncCursors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == CursorRowId, cancellationToken);

        return cursor?.LastSyncedAt ?? DateTime.MinValue;
    }

    public async Task SetLastSyncedAtAsync(DateTime value, CancellationToken cancellationToken = default)
    {
        var cursor = await _dbContext.SearchSyncCursors
            .FirstOrDefaultAsync(x => x.Id == CursorRowId, cancellationToken);

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
