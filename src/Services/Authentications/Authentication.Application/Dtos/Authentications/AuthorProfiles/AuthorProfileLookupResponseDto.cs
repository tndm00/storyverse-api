namespace Authentication.Application.Dtos.Authentications.AuthorProfiles;

/// <summary>
/// Internal, service-to-service view: maps an AuthorProfile id to the id of the
/// <see cref="Authentication.Domain.Entities.User"/> that owns it. Used by the
/// Content service to address a notification to a story's author, since Content
/// stores only the AuthorProfile id. Carries no PII.
/// </summary>
public sealed class AuthorProfileLookupResponseDto
{
    public long AuthorProfileId { get; init; }

    public long UserId { get; init; }
}
