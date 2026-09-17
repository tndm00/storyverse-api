namespace Library.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="LibraryEntry"/>. All reads are scoped to
/// the calling user id, which comes only from the validated JWT.
/// </summary>
public interface ILibraryEntryRepository
{
    /// <summary>Loads a user's library entry for a single story, or null when it isn't on their shelf.</summary>
    Task<LibraryEntry> GetAsync(long userId, Guid storyId, CancellationToken cancellationToken = default);

    /// <summary>Whether a user already has a library entry for a story.</summary>
    Task<bool> ExistsAsync(long userId, Guid storyId, CancellationToken cancellationToken = default);

    /// <summary>Paged shelf listing for a user, newest addition first, optionally filtered by shelf.</summary>
    Task<(IReadOnlyList<LibraryEntry> Items, int TotalCount)> GetPagedAsync(
        long userId,
        ShelfStatus? shelfStatus,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>Registers a new library entry to be inserted on the next <see cref="SaveChangesAsync"/>.</summary>
    Task AddAsync(LibraryEntry entry, CancellationToken cancellationToken = default);

    /// <summary>Marks a tracked library entry as modified for the next <see cref="SaveChangesAsync"/>.</summary>
    void Update(LibraryEntry entry);

    /// <summary>Marks a tracked library entry for deletion on the next <see cref="SaveChangesAsync"/>.</summary>
    void Remove(LibraryEntry entry);

    /// <summary>Persists all pending changes tracked by this repository's unit of work.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
