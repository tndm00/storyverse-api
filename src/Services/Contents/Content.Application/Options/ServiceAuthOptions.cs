namespace Content.Application.Options;

/// <summary>
/// Binds the <c>ServiceAuth</c> configuration section: the shared secret trusted
/// backend services (currently the Moderation service) present in the
/// <c>X-Service-Token</c> header when calling Content's internal endpoints
/// (moderation-visibility, title lookup). Provide the value via environment
/// variable (<c>ServiceAuth__Token</c>) or Secret Manager outside local dev,
/// per auth-guidelines.md section 12.
/// </summary>
public sealed class ServiceAuthOptions
{
    public const string SectionName = "ServiceAuth";

    /// <summary>
    /// Expected value of the inbound <c>X-Service-Token</c> header. When empty,
    /// service-token authentication is disabled and only a valid JWT is accepted.
    /// </summary>
    public string Token { get; init; } = string.Empty;
}
