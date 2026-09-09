namespace Content.Infrastructure.Repositories;

public sealed class GenreRepository : IGenreRepository
{
    private readonly ContentDbContext _dbContext;

    public GenreRepository(ContentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Genre> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Genres.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<Genre> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return _dbContext.Genres.FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
    }

    public async Task<IReadOnlyList<Genre>> GetActiveOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Genres
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Genre>> GetAllOrderedAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Genres
            .AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

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

    public Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default)
    {
        return _dbContext.Genres.AnyAsync(x => x.Name == name, cancellationToken);
    }

    public async Task AddAsync(Genre genre, CancellationToken cancellationToken = default)
    {
        await _dbContext.Genres.AddAsync(genre, cancellationToken);
    }

    public void Update(Genre genre)
    {
        _dbContext.Genres.Update(genre);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
