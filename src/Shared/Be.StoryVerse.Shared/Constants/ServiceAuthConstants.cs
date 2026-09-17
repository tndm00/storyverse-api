namespace Be.StoryVerse.Shared.Constants;

/// <summary>
/// Service-to-service auth header, shared by every service's
/// <c>ServiceOrUserAuthorizeAttribute</c> (validates it on internal
/// endpoints) and every Infrastructure HTTP client that calls another
/// service (sends it). Both sides must agree on the exact header name;
/// centralizing it here replaces what used to be a private const
/// re-declared with the same literal in each Api and Infrastructure
/// project.
/// </summary>
public static class ServiceAuthConstants
{
    public const string HeaderName = "X-Service-Token";
}
