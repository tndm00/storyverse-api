namespace Authentication.Application.Dtos.Authentications.Register;

/// <summary>
/// Response returned after successful registration. Registration also signs the
/// new account in, so an access + refresh token pair is returned (same shape as
/// <see cref="Dtos.Authentications.Sessions.LoginResponseDto"/>). Never includes
/// the password or password hash.
/// </summary>
public sealed class RegisterResponseDto
{
    public long UserId { get; init; }

    public string Email { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string AccessToken { get; init; } = string.Empty;

    public DateTimeOffset AccessTokenExpiresAt { get; init; }

    public string RefreshToken { get; init; } = string.Empty;

    public DateTimeOffset RefreshTokenExpiresAt { get; init; }

    public string TokenType { get; init; } = ApplicationConstants.BearerTokenType;
}
