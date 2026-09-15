namespace Authentication.Application.Dtos.Authentications.AuthorProfiles;

/// <summary>
/// Platform-admin request to suspend (soft-delete) or reactivate an author
/// profile. <see cref="Status"/> must be "Active" or "Suspended".
/// </summary>
public sealed class SetAuthorProfileStatusRequestDto
{
    public string Status { get; init; } = string.Empty;
}
