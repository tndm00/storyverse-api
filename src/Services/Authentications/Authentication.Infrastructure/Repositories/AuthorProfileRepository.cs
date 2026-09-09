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

    public async Task AddAsync(AuthorProfile authorProfile, CancellationToken cancellationToken = default)
    {
        await _dbContext.AuthorProfiles.AddAsync(authorProfile, cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
