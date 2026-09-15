namespace Authentication.Application.Commands.AuthorProfilesAdmin.SetAuthorProfileStatus;

/// <summary>
/// Platform-admin action: suspends (soft-delete) or reactivates an author
/// profile. Stories already published under it are untouched — this only
/// flips the profile's own lifecycle status. Guarded by <c>users.manage</c>.
/// </summary>
public sealed class SetAuthorProfileStatusCommand : ICommand<AdminAuthorProfileResponseDto>
{
    public long AuthorProfileId { get; init; }

    public string Status { get; init; } = string.Empty;
}
