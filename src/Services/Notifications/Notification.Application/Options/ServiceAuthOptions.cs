namespace Notification.Application.Options;

/// <summary>
/// Binds the <c>ServiceAuth</c> configuration section. Holds the shared secret
/// that trusted backend services (currently the Content service) present in the
/// <c>X-Service-Token</c> header when creating notifications on a user's behalf,
/// standing in for the not-yet-built event bus. Provide the value via
/// environment variable (<c>ServiceAuth__Token</c>) or Secret Manager in every
/// environment beyond local development, per auth-guidelines.md section 12.
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
