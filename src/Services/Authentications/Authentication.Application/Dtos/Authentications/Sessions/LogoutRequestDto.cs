namespace Authentication.Application.Dtos.Authentications.Sessions;

/// <summary>
/// Body of <c>POST /v1/auth/logout</c>: the refresh token to revoke. Revoking is
/// idempotent — an unknown or already-revoked token is a silent no-op.
/// </summary>
public sealed class LogoutRequestDto
{
    public string RefreshToken { get; init; } = string.Empty;
}
