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
    public const string RefreshSegment = "refresh";
    public const string LogoutSegment = "logout";
    public const string GoogleLoginSegment = "google";
    public const string MeSegment = "me";

    /// <summary>Caller's own publishing identity: <c>POST</c> to create, <c>GET</c> to read.</summary>
    public const string AuthorProfileSegment = "author-profile";

    public const string PublicAuthorSegment = "authors/{authorProfileId:long}";

    /// <summary>Service-to-service: AuthorProfile id -&gt; owning user id. X-Service-Token protected.</summary>
    public const string InternalAuthorProfileLookupSegment = "internal/author-profiles/{authorProfileId:long}";

    /// <summary>Service-to-service: batch user id -&gt; display name lookup. X-Service-Token protected.</summary>
    public const string InternalUsersLookupSegment = "internal/users";

    /// <summary>Platform-admin user administration base: <c>v1/auth/admin/users</c>.</summary>
    public const string AdminUsersBase = "v1/auth/admin/users";

    /// <summary>Grant a role: <c>POST v1/auth/admin/users/{userId}/roles</c>.</summary>
    public const string AdminUserRolesSegment = "{userId:long}/roles";

    /// <summary>Revoke a role: <c>DELETE v1/auth/admin/users/{userId}/roles/{role}</c>.</summary>
    public const string AdminUserRoleByNameSegment = "{userId:long}/roles/{role}";

    /// <summary>Platform-admin author profile administration base: <c>v1/auth/admin/authors</c>.</summary>
    public const string AdminAuthorsBase = "v1/auth/admin/authors";

    /// <summary>Update or fetch one author profile: <c>{authorProfileId}</c>.</summary>
    public const string AdminAuthorByIdSegment = "{authorProfileId:long}";

    /// <summary>Suspend/reactivate an author profile: <c>{authorProfileId}/status</c>.</summary>
    public const string AdminAuthorStatusSegment = "{authorProfileId:long}/status";
}
