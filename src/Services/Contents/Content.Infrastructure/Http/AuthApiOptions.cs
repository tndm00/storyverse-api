namespace Content.Infrastructure.Http;

/// <summary>
/// Binds <c>Services:AuthApi</c>: where the Authentication service lives and the
/// shared secret to authenticate to its internal endpoints. Both values come
/// from configuration/environment; a localhost fallback is provided for local
/// dev in appsettings.Development.json.
/// </summary>
public sealed class AuthApiOptions
{
    public const string SectionName = "Services:AuthApi";

    /// <summary>Base address of the Authentication API, e.g. <c>http://localhost:58627</c>.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Value sent in the <c>X-Service-Token</c> header. Empty disables the lookup.</summary>
    public string ServiceToken { get; init; } = string.Empty;

    /// <summary>
    /// Dev-only: skip TLS certificate validation for this client, so a call to
    /// the Authentication service's self-signed HTTPS dev endpoint succeeds.
    /// Never enable outside local development.
    /// </summary>
    public bool DangerousAcceptAnyServerCertificate { get; init; }
}
