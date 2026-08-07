namespace Authentication.Infrastructure.Security;

/// <summary>
/// Resolves the current user id strictly from the validated JWT's <c>sub</c>
/// claim on <see cref="HttpContext.User"/>, never from request body/query,
/// per auth-guidelines.md section 3.
/// </summary>
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;

    public long GetUserId()
    {
        var subClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? _httpContextAccessor.HttpContext?.User?.FindFirst(InfrastructureConstants.SubClaimType);

        if (subClaim is null || !long.TryParse(subClaim.Value, out var userId))
        {
            // Fail fast: invalid/missing identity context must not fall back to
            // an anonymous/default user, per auth-guidelines.md section 3.
            throw new ForbiddenException(InfrastructureErrorConstants.UnauthenticatedUserContext);
        }

        return userId;
    }
}
