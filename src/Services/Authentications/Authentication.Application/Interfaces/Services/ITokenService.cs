namespace Authentication.Application.Interfaces.Services;

/// <summary>
/// Issues JWT access tokens and opaque refresh tokens for an authenticated
/// <see cref="User"/>, per auth-guidelines.md sections 8-10 (Token Rules,
/// Revocation Strategy, Claims Design). Implemented in Infrastructure.
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Issues an access token for <paramref name="user"/>. When the user has a
    /// publishing identity, <paramref name="authorProfileId"/> is stamped into
    /// the <c>author_id</c> claim so content services can establish content
    /// ownership without a cross-service call; pass <c>null</c> for a
    /// reader-only account. <paramref name="roles"/> is emitted one claim per
    /// role; services expand roles to permissions locally.
    /// </summary>
    GeneratedToken GenerateAccessToken(
        User user,
        long? authorProfileId = null,
        IReadOnlyCollection<Role> roles = null);

    GeneratedToken GenerateRefreshToken();

    /// <summary>
    /// Deterministic hash of a raw refresh token value for storage and lookup.
    /// The raw value is never persisted, per auth-guidelines.md section 8.
    /// </summary>
    string HashRefreshToken(string rawRefreshToken);
}

/// <summary>
/// A generated token value plus its expiration, used for both access and
/// refresh tokens.
/// </summary>
public sealed record GeneratedToken(string Value, DateTimeOffset ExpiresAt);
