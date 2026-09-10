namespace Moderation.Infrastructure.Http;

/// <summary>
/// Base shape for a downstream service the Moderation service calls:
/// <c>BaseUrl</c> plus the shared <c>X-Service-Token</c> secret. Bound from
/// <c>Services:&lt;X&gt;Api</c>. Base appsettings leave the values empty; dev
/// points them at the sibling service's HTTPS launch-profile port.
/// </summary>
public abstract class DownstreamApiOptions
{
    public string BaseUrl { get; init; } = string.Empty;

    public string ServiceToken { get; init; } = string.Empty;

    /// <summary>Dev-only: accept the sibling service's self-signed HTTPS cert. Never enable in production.</summary>
    public bool DangerousAcceptAnyServerCertificate { get; init; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(BaseUrl) && !string.IsNullOrWhiteSpace(ServiceToken);
}

/// <summary>Binds <c>Services:ContentApi</c> — the Content service.</summary>
public sealed class ContentApiOptions : DownstreamApiOptions
{
    public const string SectionName = "Services:ContentApi";
}

/// <summary>Binds <c>Services:CommunityApi</c> — the Community service.</summary>
public sealed class CommunityApiOptions : DownstreamApiOptions
{
    public const string SectionName = "Services:CommunityApi";
}

/// <summary>Binds <c>Services:AuthApi</c> — the Authentication service.</summary>
public sealed class AuthApiOptions : DownstreamApiOptions
{
    public const string SectionName = "Services:AuthApi";
}
