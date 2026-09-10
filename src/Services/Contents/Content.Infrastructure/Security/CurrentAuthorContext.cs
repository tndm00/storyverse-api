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

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public bool IsAuthor => TryGetAuthorProfileId(out _);

    public long GetUserId()
    {
        var subClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(InfrastructureConstants.SubClaimType);

        if (subClaim is null || !long.TryParse(subClaim.Value, out var userId))
        {
            throw new ForbiddenException(ApplicationErrorConstants.CallerIsNotAuthor);
        }

        return userId;
    }

    public long GetAuthorProfileId()
    {
        if (!TryGetAuthorProfileId(out var authorProfileId))
        {
            throw new ForbiddenException(ApplicationErrorConstants.CallerIsNotAuthor);
        }

        return authorProfileId;
    }

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

    private bool TryGetAuthorProfileId(out long authorProfileId)
    {
        authorProfileId = 0;

        var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(AuthConstants.AuthorProfileIdClaimType);
        return claim is not null && long.TryParse(claim.Value, out authorProfileId) && authorProfileId > 0;
    }
}
