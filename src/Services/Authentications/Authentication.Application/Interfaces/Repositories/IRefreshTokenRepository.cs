namespace Authentication.Application.Interfaces.Repositories;

/// <summary>
/// Persistence boundary for <see cref="RefreshToken"/>. Implemented by
/// Authentication.Infrastructure, per code-standard.md section 28.
/// </summary>
public interface IRefreshTokenRepository
{
    /// <summary>The stored token row for this raw value's hash, or null when unknown.</summary>
    Task<RefreshToken> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default);

    void Update(RefreshToken refreshToken);

    /// <summary>
    /// Atomically revokes every still-active token for a user. Used on
    /// reuse-detection (a rotated token was replayed) to drop the whole family.
    /// Returns the number of rows revoked. The caller does not need to
    /// <see cref="SaveChangesAsync"/> — this issues the UPDATE directly.
    /// </summary>
    Task<int> RevokeAllActiveForUserAsync(long userId, DateTime revokedAtUtc, CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
