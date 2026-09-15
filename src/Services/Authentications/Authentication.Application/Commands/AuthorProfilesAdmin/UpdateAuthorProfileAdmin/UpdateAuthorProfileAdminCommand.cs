namespace Authentication.Application.Commands.AuthorProfilesAdmin.UpdateAuthorProfileAdmin;

/// <summary>Platform-admin action: edits an existing author profile. Guarded by <c>users.manage</c>.</summary>
public sealed class UpdateAuthorProfileAdminCommand : ICommand<AdminAuthorProfileResponseDto>
{
    public long AuthorProfileId { get; init; }

    public string PenName { get; init; } = string.Empty;

    public string Bio { get; init; }

    public string AvatarUrl { get; init; }

    public string BannerUrl { get; init; }

    public bool Verified { get; init; }
}
