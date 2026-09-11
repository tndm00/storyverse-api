using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Community.Infrastructure.Http;

/// <summary>
/// Request-scoped typed <see cref="HttpClient"/> for the Content service's
/// internal <c>POST /v1/stories/{storyId}/rating-summary</c> endpoint. Every
/// failure path (not configured, non-success response, network error) is
/// swallowed and logged as a warning — the caller's rating write already
/// committed and must not be affected.
/// </summary>
public sealed class ContentRatingSyncClient : IContentRatingSyncClient
{
    private const string ServiceTokenHeader = "X-Service-Token";

    private readonly HttpClient _httpClient;
    private readonly ContentApiOptions _options;
    private readonly ILogger<ContentRatingSyncClient> _logger;

    public ContentRatingSyncClient(
        HttpClient httpClient,
        IOptions<ContentApiOptions> options,
        ILogger<ContentRatingSyncClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SyncRatingSummaryAsync(
        Guid storyId, decimal ratingAvg, int ratingCount, CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            _logger.LogWarning(
                "Content API not configured (Services:ContentApi); story {StoryId} rating summary was not synced.",
                storyId);
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"v1/stories/{storyId}/rating-summary")
            {
                Content = JsonContent.Create(new { ratingAvg, ratingCount })
            };
            request.Headers.Add(ServiceTokenHeader, _options.ServiceToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex, "Rating summary sync to Content failed for story {StoryId}; Story.RatingAvg/RatingCount stay stale.",
                storyId);
        }
    }

    private bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(_options.BaseUrl) && !string.IsNullOrWhiteSpace(_options.ServiceToken);
}
