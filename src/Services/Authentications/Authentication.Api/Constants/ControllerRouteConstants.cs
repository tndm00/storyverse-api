namespace Authentication.Api.Constants;

/// <summary>
/// Centralized route segments, per code-standard.md section 11 (Constants Rules)
/// and api-guidelines.md sections 3-4 (Versioning, URI Design).
/// </summary>
public static class ControllerRouteConstants
{
    public const string ApiVersion1 = "v1";
    public const string AuthBase = "v1/auth";

    public const string RegisterSegment = "register";
    public const string LoginSegment = "login";
    public const string GoogleLoginSegment = "google";
    public const string MeSegment = "me";

    /// <summary>Caller's own publishing identity: <c>POST</c> to create, <c>GET</c> to read.</summary>
    public const string AuthorProfileSegment = "author-profile";

    public const string PublicAuthorSegment = "authors/{authorProfileId:long}";

    /// <summary>Service-to-service: AuthorProfile id -&gt; owning user id. X-Service-Token protected.</summary>
    public const string InternalAuthorProfileLookupSegment = "internal/author-profiles/{authorProfileId:long}";
}
