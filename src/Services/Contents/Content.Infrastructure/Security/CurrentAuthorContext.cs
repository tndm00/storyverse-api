namespace Content.Infrastructure.Security;

/// <summary>
/// Resolves the caller's identity strictly from the validated JWT on
/// <see cref="HttpContext.User"/>, never from request input, per
/// auth-guidelines.md section 3.
/// </summary>
public sealed class CurrentAuthorContext : ICurrentAuthorContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentAuthorContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>Whether the current request has an authenticated user.</summary>
    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <summary>Whether the current user carries a valid author profile id claim.</summary>
    public bool IsAuthor => TryGetAuthorProfileId(out _);

    /// <summary>
    /// Resolves the caller's user id from the JWT subject claim. Throws
    /// <see cref="ForbiddenException"/> if the claim is missing or not a valid id.
    /// </summary>
    public long GetUserId()
    {
        // Prefer the standard JWT sub claim; fall back to the raw claim name if it
        // wasn't remapped during token validation.
        var subClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(InfrastructureConstants.SubClaimType);

        if (subClaim is null || !long.TryParse(subClaim.Value, out var userId))
        {
            throw new ForbiddenException(ApplicationErrorConstants.CallerIsNotAuthor);
        }

        return userId;
    }

    /// <summary>
    /// Resolves the caller's author profile id. Throws <see cref="ForbiddenException"/>
    /// if the caller does not have a valid author profile claim.
    /// </summary>
    public long GetAuthorProfileId()
    {
        if (!TryGetAuthorProfileId(out var authorProfileId))
        {
            throw new ForbiddenException(ApplicationErrorConstants.CallerIsNotAuthor);
        }

        return authorProfileId;
    }

    /// <summary>
    /// Checks whether the caller's roles (from the JWT) grant the given permission,
    /// via the shared role-to-permission map.
    /// </summary>
    public bool HasPermission(string permission)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user is null)
        {
            return false;
        }

        var roles = user.FindAll(AuthConstants.RolesClaimType).Select(c => c.Value);
        return Be.StoryVerse.Shared.Authorization.RolePermissionMap.PermissionsFor(roles).Contains(permission);
    }

    /// <summary>Attempts to read and parse the author profile id claim from the current user.</summary>
    private bool TryGetAuthorProfileId(out long authorProfileId)
    {
        authorProfileId = 0;

        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(AuthConstants.AuthorProfileIdClaimType);
        return claim is not null && long.TryParse(claim.Value, out authorProfileId) && authorProfileId > 0;
    }
}
