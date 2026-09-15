namespace Authentication.Infrastructure.Repositories;

public sealed class AuthorProfileRepository : IAuthorProfileRepository
{
    private readonly AuthenticationDbContext _dbContext;

    public AuthorProfileRepository(AuthenticationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<AuthorProfile> GetByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuthorProfiles.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<AuthorProfile> GetByIdAsync(long authorProfileId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuthorProfiles.FirstOrDefaultAsync(x => x.Id == authorProfileId, cancellationToken);
    }

    public Task<bool> ExistsByUserIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        return _dbContext.AuthorProfiles.AnyAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<(IReadOnlyList<AuthorProfile> Items, int TotalCount)> GetPagedAsync(
        string keyword,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.AuthorProfiles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var pattern = $"%{keyword.Trim()}%";
            query = query.Where(x => EF.Functions.ILike(x.PenName, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(AuthorProfile authorProfile, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuthorProfiles.AddAsync(authorProfile, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
