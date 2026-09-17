namespace Library.Infrastructure.Repositories;

/// <summary>EF Core-backed repository for <see cref="LibraryEntry"/> rows, scoped per request via the shared DbContext.</summary>
public sealed class LibraryEntryRepository : ILibraryEntryRepository
{
    private readonly LibraryDbContext _dbContext;

    public LibraryEntryRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Fetches a reader's library entry for a story, tracked, or null if none exists.</summary>
    public Task<LibraryEntry> GetAsync(long userId, Guid storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.LibraryEntries
            .FirstOrDefaultAsync(x => x.UserId == userId && x.StoryId == storyId, cancellationToken);
    }

    /// <summary>Checks whether a reader already has a library entry for the given story.</summary>
    public Task<bool> ExistsAsync(long userId, Guid storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.LibraryEntries
            .AnyAsync(x => x.UserId == userId && x.StoryId == storyId, cancellationToken);
    }

    /// <summary>Returns a page of a reader's library entries, optionally filtered by shelf, newest-added first.</summary>
    public async Task<(IReadOnlyList<LibraryEntry> Items, int TotalCount)> GetPagedAsync(
        long userId,
        ShelfStatus? shelfStatus,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.LibraryEntries
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        // Optional shelf filter, e.g. only "Reading" or "Completed" entries.
        if (shelfStatus is { } status)
        {
            query = query.Where(x => x.ShelfStatus == status);
        }

        // Total count before paging, for the caller's PagedResponseDto.
        var totalCount = await query.CountAsync(cancellationToken);

        // Newest-added first, with a stable tiebreaker for equal timestamps.
        var items = await query
            .OrderByDescending(x => x.AddedAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>Stages a new library entry for insertion; not persisted until <see cref="SaveChangesAsync"/>.</summary>
    public async Task AddAsync(LibraryEntry entry, CancellationToken cancellationToken = default)
    {
        await _dbContext.LibraryEntries.AddAsync(entry, cancellationToken);
    }

    /// <summary>Marks a library entry as modified; not persisted until <see cref="SaveChangesAsync"/>.</summary>
    public void Update(LibraryEntry entry)
    {
        _dbContext.LibraryEntries.Update(entry);
    }

    /// <summary>Stages a library entry for deletion; not persisted until <see cref="SaveChangesAsync"/>.</summary>
    public void Remove(LibraryEntry entry)
    {
        _dbContext.LibraryEntries.Remove(entry);
    }

    /// <summary>Persists all pending changes tracked by the DbContext.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
