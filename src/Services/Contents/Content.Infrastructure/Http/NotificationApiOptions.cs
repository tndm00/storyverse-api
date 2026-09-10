namespace Content.Infrastructure.Http;

/// <summary>
/// Binds <c>Services:NotificationApi</c>: where the Notification service lives
/// and the shared secret to authenticate to it. Both values come from
/// configuration/environment; a localhost fallback is provided for local dev in
/// appsettings.Development.json.
/// </summary>
public sealed class NotificationApiOptions
{
    public const string SectionName = "Services:NotificationApi";

    /// <summary>Base address of the Notification API, e.g. <c>http://localhost:52179</c>.</summary>
    public string BaseUrl { get; init; } = string.Empty;

    /// <summary>Value sent in the <c>X-Service-Token</c> header. Empty disables outbound notifications.</summary>
    public string ServiceToken { get; init; } = string.Empty;

    /// <summary>
    /// Dev-only: skip TLS certificate validation for this client, so a call to a
    /// sibling service's self-signed HTTPS dev endpoint succeeds. Never enable
    /// outside local development.
    /// </summary>
    public bool DangerousAcceptAnyServerCertificate { get; init; }
}
