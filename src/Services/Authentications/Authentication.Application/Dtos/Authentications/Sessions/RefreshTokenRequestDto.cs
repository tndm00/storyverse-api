namespace Authentication.Application.Dtos.Authentications.Sessions;

/// <summary>
/// Body of <c>POST /v1/auth/refresh</c>: the opaque refresh token issued on the
/// last login or refresh.
/// </summary>
public sealed class RefreshTokenRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
