namespace Library.Infrastructure.Repositories;

public sealed class LibraryEntryRepository : ILibraryEntryRepository
{
    private readonly LibraryDbContext _dbContext;

    public LibraryEntryRepository(LibraryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<LibraryEntry> GetAsync(long userId, Guid storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.LibraryEntries
            .FirstOrDefaultAsync(x => x.UserId == userId && x.StoryId == storyId, cancellationToken);
    }

    public Task<bool> ExistsAsync(long userId, Guid storyId, CancellationToken cancellationToken = default)
    {
        return _dbContext.LibraryEntries
            .AnyAsync(x => x.UserId == userId && x.StoryId == storyId, cancellationToken);
    }

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

        if (shelfStatus is { } status)
        {
            query = query.Where(x => x.ShelfStatus == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.AddedAt)
            .ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(LibraryEntry entry, CancellationToken cancellationToken = default)
    {
        await _dbContext.LibraryEntries.AddAsync(entry, cancellationToken);
    }

    public void Update(LibraryEntry entry)
    {
        _dbContext.LibraryEntries.Update(entry);
    }

    public void Remove(LibraryEntry entry)
    {
        _dbContext.LibraryEntries.Remove(entry);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
