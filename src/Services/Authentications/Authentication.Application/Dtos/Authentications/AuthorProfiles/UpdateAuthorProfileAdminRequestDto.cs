namespace Authentication.Application.Dtos.Authentications.AuthorProfiles;

/// <summary>Platform-admin request to edit an existing author profile's fields.</summary>
public sealed class UpdateAuthorProfileAdminRequestDto
{
    public string PenName { get; init; } = string.Empty;

    public string Bio { get; init; }

    public string AvatarUrl { get; init; }

    public string BannerUrl { get; init; }

    public bool Verified { get; init; }
}
