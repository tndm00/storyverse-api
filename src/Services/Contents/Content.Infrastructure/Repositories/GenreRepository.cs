namespace Content.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IGenreRepository"/> for the admin-managed
/// genre reference list.
/// </summary>
public sealed class GenreRepository : IGenreRepository
{
    private readonly ContentDbContext _dbContext;

    public GenreRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Looks up a genre by its numeric id.</summary>
    public Task<Genre> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Genres.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    /// <summary>Looks up a genre by its URL slug.</summary>
    public Task<Genre> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _dbContext.Genres.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }

    /// <summary>Lists all active genres in display order, for populating public-facing genre pickers.</summary>
    public async Task<IReadOnlyList<Genre>> GetActiveOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Genres
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Lists every genre (active and inactive) in display order, for admin management.</summary>
    public async Task<IReadOnlyList<Genre>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Genres
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Resolves a set of slugs to their active genres, e.g. when validating a story's genre selection.</summary>
    public async Task<IReadOnlyList<Genre>> GetActiveBySlugsAsync(
        IReadOnlyCollection<string> slugs,
        CancellationToken cancellationToken = default)
    {
        if (slugs.Count == 0)
        {
            return Array.Empty<Genre>();
        }

        return await _dbContext.Genres
            .Where(x => x.IsActive && slugs.Contains(x.Slug))
            .ToListAsync(cancellationToken);
    }

    /// <summary>Checks whether a genre with the given name already exists, for uniqueness validation.</summary>
    public Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return _dbContext.Genres.AnyAsync(x => x.Name == name, cancellationToken);
    }

    /// <summary>Queues a new genre for insertion; call <see cref="SaveChangesAsync"/> to persist.</summary>
    public async Task AddAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        await _dbContext.Genres.AddAsync(genre, cancellationToken);
    }

    /// <summary>Marks a tracked genre as modified; call <see cref="SaveChangesAsync"/> to persist.</summary>
    public void Update(Genre genre)
    {
        _dbContext.Genres.Update(genre);
    }

    /// <summary>Persists all pending changes tracked by the context.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
