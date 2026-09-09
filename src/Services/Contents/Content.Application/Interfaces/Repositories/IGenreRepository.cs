namespace Content.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for the admin-managed <see cref="Genre"/> list.
/// </summary>
public interface IGenreRepository
{
    Task<Genre> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<Genre> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Genre>> GetActiveOrderedAsync(CancellationToken cancellationToken = default);

    /// <summary>Every genre (active and hidden), ordered for display. Admin management page.</summary>
    Task<IReadOnlyList<Genre>> GetAllOrderedAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the active genres among the given slugs; used to validate a story's genre selection.</summary>
    Task<IReadOnlyList<Genre>> GetActiveBySlugsAsync(
        IReadOnlyCollection<string> slugs,
        CancellationToken cancellationToken = default);

    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);

    Task AddAsync(Genre genre, CancellationToken cancellationToken = default);

    void Update(Genre genre);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
