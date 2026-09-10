namespace Authentication.Application.Dtos.Authentications.Directory;

/// <summary>
/// Internal, service-to-service view: an account id paired with its public
/// display name. Used by Moderation and Community to turn stored numeric user
/// ids into reader-facing names. Carries no email or other PII.
/// </summary>
public sealed class UserDirectoryEntryDto
{
    public long UserId { get; init; }

    public string DisplayName { get; init; } = string.Empty;
}
