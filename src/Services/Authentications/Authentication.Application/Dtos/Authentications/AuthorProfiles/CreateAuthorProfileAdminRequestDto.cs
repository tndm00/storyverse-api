namespace Authentication.Application.Dtos.Authentications.AuthorProfiles;

/// <summary>
/// Platform-admin request to create a brand-new account plus its author
/// profile in one step (unlike the self-service flow, which requires an
/// existing signed-in account).
/// </summary>
public sealed class CreateAuthorProfileAdminRequestDto
{
    public string Email { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public string PenName { get; init; } = string.Empty;

    public string Bio { get; init; }
}
