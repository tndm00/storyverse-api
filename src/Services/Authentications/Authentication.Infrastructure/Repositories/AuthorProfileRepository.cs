namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IAuthorProfileRepository"/>.
/// </summary>
public sealed class AuthorProfileRepository : IAuthorProfileRepository
{
    private readonly AuthenticationDbContext _dbContext;

    /// <summary>
    /// Creates the repository bound to the given <see cref="AuthenticationDbContext"/>.
    /// </summary>
    public AuthorProfileRepository(AuthenticationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Looks up an author profile by owning user id, or null if the user has none.
    /// </summary>
    public Task<AuthorProfile> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuthorProfiles.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Looks up an author profile by its own id.
    /// </summary>
    public Task<AuthorProfile> GetByIdAsync(long authorProfileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuthorProfiles.FirstOrDefaultAsync(x => x.Id == authorProfileId, cancellationToken);
    }

    /// <summary>
    /// Checks whether the given user already has an author profile.
    /// </summary>
    public Task<bool> ExistsByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuthorProfiles.AnyAsync(x => x.UserId == userId, cancellationToken);
    }

    /// <summary>
    /// Returns a page of author profiles, optionally filtered by pen-name keyword,
    /// ordered newest first, plus the total matching count.
    /// </summary>
    public async Task<(IReadOnlyList<AuthorProfile> Items, int TotalCount)> GetPagedAsync(
        string keyword,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuthorProfiles.AsQueryable();

        // Case-insensitive substring match on pen name when a keyword is supplied.
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.PenName, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply paging after the count so TotalCount reflects the full filtered set.
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Stages a new author profile for insertion; persisted on <see cref="SaveChangesAsync"/>.
    /// </summary>
    public async Task AddAsync(AuthorProfile authorProfile, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuthorProfiles.AddAsync(authorProfile, cancellationToken);
    }

    /// <summary>
    /// Persists all pending changes tracked by the context.
    /// </summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
