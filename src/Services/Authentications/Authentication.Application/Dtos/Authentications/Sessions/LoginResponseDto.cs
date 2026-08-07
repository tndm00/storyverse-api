namespace Authentication.Application.Dtos.Authentications.Sessions;

/// <summary>
/// Tokens issued on successful login, per auth-guidelines.md section 8 (Token Rules).
/// </summary>
public sealed class LoginResponseDto
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTimeOffset AccessTokenExpiresAt { get; init; }

    public string RefreshToken { get; init; } = string.Empty;

    public DateTimeOffset RefreshTokenExpiresAt { get; init; }

    public string TokenType { get; init; } = ApplicationConstants.BearerTokenType;
}
