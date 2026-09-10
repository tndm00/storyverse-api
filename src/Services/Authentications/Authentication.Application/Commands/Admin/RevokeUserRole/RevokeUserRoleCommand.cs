namespace Authentication.Application.Commands.Admin.RevokeUserRole;

/// <summary>
/// Platform-admin action: removes one platform role grant from an account.
/// Idempotent — revoking a role the account does not hold is a no-op.
/// <c>Reader</c> is the implicit floor and cannot be revoked. Guarded by
/// <c>users.manage</c>.
/// </summary>
public sealed class RevokeUserRoleCommand : ICommand<UserRolesResponseDto>
{
    public long UserId { get; init; }

    public string Role { get; init; } = string.Empty;
}
