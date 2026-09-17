namespace Library.Infrastructure.Security;

/// <summary>
/// Resolves the caller's identity strictly from the validated JWT on
/// <see cref="HttpContext.User"/>, never from request input, per
/// auth-guidelines.md section 3. Reads only the <c>sub</c> claim — the Library
/// service scopes every row to a user, not an author profile.
/// </summary>
public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    /// <summary>Extracts and parses the caller's user id from the JWT <c>sub</c> claim, or throws if missing/invalid.</summary>
    public long GetUserId()
    {
        // Standard claim name first, falling back to the raw "sub" claim if it was not remapped.
        var subClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(InfrastructureConstants.SubClaimType);

        // No unauthenticated or malformed callers get past this point.
        if (subClaim is null || !long.TryParse(subClaim.Value, out var userId))
        {
            throw new ForbiddenException(ApplicationErrorConstants.CallerNotAuthenticated);
        }

        return userId;
    }
}
