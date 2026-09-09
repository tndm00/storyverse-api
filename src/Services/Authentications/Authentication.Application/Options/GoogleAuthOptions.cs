namespace Authentication.Application.Options;

/// <summary>
/// Binds the <c>GcpSettings:GoogleAuthSettings</c> configuration section. The
/// OAuth client id is a public value (it is embedded in the frontend), so it may
/// live in <c>appsettings.json</c>; the client secret is not used by the
/// ID-token verification flow and must never be committed, per
/// auth-guidelines.md section 12.
/// </summary>
public sealed class GoogleAuthOptions
{
    public const string SectionName = "GcpSettings:GoogleAuthSettings";

    /// <summary>
    /// Google OAuth 2.0 client id. Every Google ID token presented to the API
    /// must carry this value as its audience, or it is rejected.
    /// </summary>
    public string ClientId { get; init; } = string.Empty;
}
