namespace Authentication.Application.Commands.AuthorProfilesAdmin.CreateAuthorProfileAdmin;

/// <summary>
/// Platform-admin action: creates a brand-new account and its author profile
/// in one step. Guarded by <c>users.manage</c>.
/// </summary>
public sealed class CreateAuthorProfileAdminCommand : ICommand<AdminAuthorProfileResponseDto>
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string PenName { get; init; } = string.Empty;

    public string Bio { get; init; }
}
