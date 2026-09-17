namespace Content.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for the admin-managed <see cref="Genre"/> list.
/// </summary>
public interface IGenreRepository
{
    /// <summary>Loads a single genre by its internal id.</summary>
    Task<Genre> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    /// <summary>Loads a single genre by its slug.</summary>
    Task<Genre> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    /// <summary>Active (non-hidden) genres, ordered for display. Public genre picker.</summary>
    Task<IReadOnlyList<Genre>> GetActiveOrderedAsync(CancellationToken cancellationToken = default);

    /// <summary>Every genre (active and hidden), ordered for display. Admin management page.</summary>
    Task<IReadOnlyList<Genre>> GetAllOrderedAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns the active genres among the given slugs; used to validate a story's genre selection.</summary>
    Task<IReadOnlyList<Genre>> GetActiveBySlugsAsync(
        IReadOnlyCollection<string> slugs,
        CancellationToken cancellationToken = default);

    /// <summary>Whether a genre with this name already exists.</summary>
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>Registers a new genre to be inserted on the next <see cref="SaveChangesAsync"/>.</summary>
    Task AddAsync(Genre genre, CancellationToken cancellationToken = default);

    /// <summary>Marks a tracked genre as modified for the next <see cref="SaveChangesAsync"/>.</summary>
    void Update(Genre genre);

    /// <summary>Persists all pending changes tracked by this repository's unit of work.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
