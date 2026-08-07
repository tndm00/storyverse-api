namespace Authentication.Application.Interfaces.Services;

/// <summary>
/// Issues JWT access tokens and opaque refresh tokens for an authenticated
/// <see cref="User"/>, per auth-guidelines.md sections 8-10 (Token Rules,
/// Revocation Strategy, Claims Design). Implemented in Infrastructure.
/// </summary>
public interface ITokenService
{
    GeneratedToken GenerateAccessToken(User user);

    GeneratedToken GenerateRefreshToken();
}

/// <summary>
/// A generated token value plus its expiration, used for both access and
/// refresh tokens.
/// </summary>
public sealed record GeneratedToken(string Value, DateTimeOffset ExpiresAt);
