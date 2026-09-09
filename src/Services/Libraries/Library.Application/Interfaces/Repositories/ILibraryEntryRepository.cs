namespace Library.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="LibraryEntry"/>. All reads are scoped to
/// the calling user id, which comes only from the validated JWT.
/// </summary>
public interface ILibraryEntryRepository
{
    Task<LibraryEntry> GetAsync(long userId, Guid storyId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(long userId, Guid storyId, CancellationToken cancellationToken = default);

    /// <summary>Paged shelf listing for a user, newest addition first, optionally filtered by shelf.</summary>
    Task<(IReadOnlyList<LibraryEntry> Items, int TotalCount)> GetPagedAsync(
        long userId,
        ShelfStatus? shelfStatus,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task AddAsync(LibraryEntry entry, CancellationToken cancellationToken = default);

    void Update(LibraryEntry entry);

    void Remove(LibraryEntry entry);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
