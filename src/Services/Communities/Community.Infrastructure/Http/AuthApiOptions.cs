namespace Community.Infrastructure.Http;

/// <summary>
/// Binds <c>Services:AuthApi</c>: where the Authentication service lives and the
/// shared secret for its internal endpoints. A missing value disables the
/// display-name lookup (comment/rating listings fall back to the numeric id).
/// </summary>
public sealed class AuthApiOptions
{
    public const string SectionName = "Services:AuthApi";

    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Value sent in the <c>X-Service-Token</c> header. Empty disables the lookup.</summary>
    public string ServiceToken { get; init; } = string.Empty;

    /// <summary>Dev-only: accept the sibling service's self-signed HTTPS cert. Never enable in production.</summary>
    public bool DangerousAcceptAnyServerCertificate { get; init; }
}
