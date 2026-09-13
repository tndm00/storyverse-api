namespace Content.Infrastructure.Http;

/// <summary>
/// Binds the <c>FacebookIntegration</c> configuration section: the Facebook Page
/// this service posts new-story/new-chapter announcements to via the Graph API.
/// A missing <see cref="PageId"/> or <see cref="PageAccessToken"/>, or
/// <see cref="Enabled"/>=false, short-circuits posting to a no-op.
/// </summary>
public sealed class FacebookIntegrationOptions
{
    public const string SectionName = "FacebookIntegration";

    /// <summary>Master switch for outbound Facebook posting. Defaults to disabled until configured.</summary>
    public bool Enabled { get; init; }

    public string PageId { get; init; } = string.Empty;

    /// <summary>Page Access Token from Meta Business Suite / Graph API Explorer. Empty disables posting.</summary>
    public string PageAccessToken { get; init; } = string.Empty;

    public string GraphApiBaseUrl { get; init; } = "https://graph.facebook.com/v19.0/";
}
