namespace Authentication.Application.Commands.CreateAuthorProfile;

/// <summary>
/// Turns the authenticated caller into an author by creating their
/// <see cref="AuthorProfile"/> (Author Onboarding Flow step 2,
/// product-workflow-context.md section 5.1). One profile per user; reader
/// capabilities are unaffected.
/// </summary>
public sealed class CreateAuthorProfileCommand : ICommand<AuthorProfileResponseDto>
{
    public string PenName { get; init; } = string.Empty;

    public string Bio { get; init; }

    public string AvatarUrl { get; init; }

    public string BannerUrl { get; init; }
}
