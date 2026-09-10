namespace Authentication.Application.Commands.Admin.GrantUserRole;

/// <summary>
/// Platform-admin action: grants one platform role to an account. Idempotent —
/// re-granting an already-held role is a no-op that still returns the current
/// role set. Guarded by <c>users.manage</c>.
/// </summary>
public sealed class GrantUserRoleCommand : ICommand<UserRolesResponseDto>
{
    public long UserId { get; init; }

    public string Role { get; init; } = string.Empty;
}
