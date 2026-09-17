namespace Community.Infrastructure.Security;

/// <summary>
/// Resolves the caller's identity strictly from the validated JWT on
/// <see cref="HttpContext.User"/>, never from request input, per
/// auth-guidelines.md section 3. Mirrors the Content service's
/// <c>CurrentAuthorContext</c>.
/// </summary>
public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    /// <summary>Creates the context with the accessor used to reach the current HTTP request's principal.</summary>
    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <summary>Reads the authenticated user's id from the JWT <c>sub</c> claim; throws if missing/invalid.</summary>
    public long GetUserId()
    {
        // Try the mapped registered claim first, then the raw "sub" claim as a fallback.
        var subClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(InfrastructureConstants.SubClaimType);

        // No valid positive user id: caller is not properly authenticated.
        if (subClaim is null || !long.TryParse(subClaim.Value, out var userId) || userId <= 0)
        {
            throw new ForbiddenException(ApplicationErrorConstants.UserNotAuthenticated);
        }

        return userId;
    }
}
