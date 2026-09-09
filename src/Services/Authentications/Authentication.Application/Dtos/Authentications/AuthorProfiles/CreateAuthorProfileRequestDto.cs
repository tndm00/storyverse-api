namespace Authentication.Application.Dtos.Authentications.AuthorProfiles;

/// <summary>
/// Request body for creating the caller's publishing identity (Author
/// Onboarding Flow step 2, product-workflow-context.md section 5.1). The owning
/// user is resolved from the JWT, never from this payload.
/// </summary>
public sealed class CreateAuthorProfileRequestDto
{
    public string PenName { get; init; } = string.Empty;

    public string Bio { get; init; }

    public string AvatarUrl { get; init; }

    public string BannerUrl { get; init; }
}
