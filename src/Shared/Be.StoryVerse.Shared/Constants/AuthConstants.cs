namespace Be.StoryVerse.Shared.Constants;

/// <summary>
/// Stable auth-scheme constants reused across services and layers (Api Swagger
/// wiring, Application-layer token response shaping, and any future service's
/// auth setup), per code-standard.md section 11 (Constants Rules) Placement
/// table — "Shared headers, base error codes, common response constants".
/// </summary>
public static class AuthConstants
{
    public const string BearerScheme = "Bearer";

    /// <summary>
    /// JWT claim carrying the caller's AuthorProfile id. The AuthorProfile row is
    /// owned by the Authentication service; content services read this claim to
    /// establish content ownership without a cross-service call. Both the token
    /// issuer and every consumer must agree on this exact claim name.
    /// </summary>
    public const string AuthorProfileIdClaimType = "author_id";

    /// <summary>
    /// JWT claim carrying one of the caller's role names (emitted once per role).
    /// Services expand these to permissions via
    /// <see cref="Authorization.RolePermissionMap"/>; the token itself never
    /// carries permissions.
    /// </summary>
    public const string RolesClaimType = "roles";
}
