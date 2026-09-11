namespace Community.Infrastructure.Http;

/// <summary>
/// Binds <c>Services:ContentApi</c>: where the Content service lives and the
/// shared secret for its internal endpoints. A missing value disables the
/// rating-summary sync (the Content-side denormalized avg/count are simply not
/// refreshed; the rating write itself is unaffected).
/// </summary>
public sealed class ContentApiOptions
{
    public const string SectionName = "Services:ContentApi";

    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Value sent in the <c>X-Service-Token</c> header. Empty disables the sync.</summary>
    public string ServiceToken { get; init; } = string.Empty;

    /// <summary>Dev-only: accept the sibling service's self-signed HTTPS cert. Never enable in production.</summary>
    public bool DangerousAcceptAnyServerCertificate { get; init; }
}
