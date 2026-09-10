namespace Authentication.Application.Commands.Logout;

/// <summary>
/// Revokes the caller's current refresh token so it can no longer be exchanged.
/// Idempotent: an unknown, already-revoked, or foreign token is a silent no-op.
/// </summary>
public sealed class LogoutCommand : ICommand<Unit>
{
    public string RefreshToken { get; init; } = string.Empty;
}
