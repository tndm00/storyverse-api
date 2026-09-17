namespace Notification.Infrastructure.Security;

/// <summary>
/// Resolves the caller's identity strictly from the validated JWT on
/// <see cref="HttpContext.User"/>, never from request input, per
/// auth-guidelines.md section 3. Mirrors the Content service's
/// <c>CurrentAuthorContext</c>.
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

    /// <summary>Reads the caller's user id from the JWT <c>sub</c> claim; throws if missing or invalid.</summary>
    public long GetUserId()
    {
        // Look for the canonical registered claim first, falling back to the raw "sub" claim type.
        var subClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(InfrastructureConstants.SubClaimType);

        // Reject anything that isn't a usable positive numeric subject.
        if (subClaim is null || !long.TryParse(subClaim.Value, out var userId) || userId <= 0)
        {
            throw new ForbiddenException(ApplicationErrorConstants.CallerNotAuthenticated);
        }

        return userId;
    }
}
