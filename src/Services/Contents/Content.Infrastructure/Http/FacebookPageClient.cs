using System.Net.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Content.Infrastructure.Http;

/// <summary>
/// Typed <see cref="HttpClient"/> that posts to the configured Facebook Page's
/// feed via the Graph API. Best-effort by contract: it does not retry and
/// callers are expected to catch. A missing <see cref="FacebookIntegrationOptions.PageId"/>
/// or <see cref="FacebookIntegrationOptions.PageAccessToken"/>, or
/// <see cref="FacebookIntegrationOptions.Enabled"/>=false, short-circuits to a no-op.
/// </summary>
public sealed class FacebookPageClient : IFacebookPageClient
{
    private readonly HttpClient _httpClient;
    private readonly FacebookIntegrationOptions _options;
    private readonly ILogger<FacebookPageClient> _logger;

    public FacebookPageClient(
        HttpClient httpClient,
        IOptions<FacebookIntegrationOptions> options,
        ILogger<FacebookPageClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task PostAsync(string message, string link, CancellationToken cancellationToken)
    {
        if (!_options.Enabled ||
            string.IsNullOrWhiteSpace(_options.PageId) ||
            string.IsNullOrWhiteSpace(_options.PageAccessToken))
        {
            _logger.LogWarning(
                "Facebook integration not configured or disabled (Enabled/PageId/PageAccessToken); skipped post.");
            return;
        }

        var form = new Dictionary<string, string>
        {
            ["message"] = message,
            ["link"] = link,
            ["access_token"] = _options.PageAccessToken
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"{_options.PageId}/feed")
        {
            Content = new FormUrlEncodedContent(form)
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
