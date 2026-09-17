namespace Authentication.Infrastructure.Repositories;

/// <summary>
/// EF Core-backed implementation of <see cref="IRefreshTokenRepository"/>.
/// </summary>
public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AuthenticationDbContext _dbContext;

    /// <summary>
    /// Creates the repository bound to the given <see cref="AuthenticationDbContext"/>.
    /// </summary>
    public RefreshTokenRepository(AuthenticationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Looks up a refresh token row by the hash of its raw value.
    /// </summary>
    public Task<RefreshToken> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);
    }

    /// <summary>
    /// Stages a new refresh token for insertion; persisted on <see cref="SaveChangesAsync"/>.
    /// </summary>
    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _dbContext.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    /// <summary>
    /// Marks an already-tracked refresh token as modified (e.g. after rotation/revocation).
    /// </summary>
    public void Update(RefreshToken refreshToken)
    {
        _dbContext.RefreshTokens.Update(refreshToken);
    }

    /// <summary>
    /// Bulk-revokes every currently active (non-revoked) refresh token for a user,
    /// e.g. on logout-all or reuse detection. Bypasses change tracking via ExecuteUpdate.
    /// </summary>
    public Task<int> RevokeAllActiveForUserAsync(
        long userId, DateTime revokedAtUtc, CancellationToken cancellationToken = default)
    {
        return _dbContext.RefreshTokens
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.RevokedAt, revokedAtUtc), cancellationToken);
    }

    /// <summary>
    /// Persists all pending changes tracked by the context.
    /// </summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
