using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Community.Infrastructure.Http;

/// <summary>
/// Request-scoped typed <see cref="HttpClient"/> for the Content service's
/// internal <c>POST /v1/chapters/{chapterId}/comment-count</c> endpoint. Every
/// failure path (not configured, non-success response, network error) is
/// swallowed and logged as a warning — the caller's comment write already
/// committed and must not be affected. Mirrors <see cref="ContentRatingSyncClient"/>.
/// </summary>
public sealed class ContentCommentCountSyncClient : IContentCommentCountSyncClient
{
    private const string ServiceTokenHeader = "X-Service-Token";

    private readonly HttpClient _httpClient;
    private readonly ContentApiOptions _options;
    private readonly ILogger<ContentCommentCountSyncClient> _logger;

    public ContentCommentCountSyncClient(
        HttpClient httpClient,
        IOptions<ContentApiOptions> options,
        ILogger<ContentCommentCountSyncClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SyncCommentCountAsync(Guid chapterId, int commentCount, CancellationToken cancellationToken)
    {
        if (!IsConfigured())
        {
            _logger.LogWarning(
                "Content API not configured (Services:ContentApi); chapter {ChapterId} comment count was not synced.",
                chapterId);
            return;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, $"v1/chapters/{chapterId}/comment-count")
            {
                Content = JsonContent.Create(new { count = commentCount })
            };
            request.Headers.Add(ServiceTokenHeader, _options.ServiceToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex, "Comment count sync to Content failed for chapter {ChapterId}; Chapter.CommentCount stays stale.",
                chapterId);
        }
    }

    private bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(_options.BaseUrl) && !string.IsNullOrWhiteSpace(_options.ServiceToken);
}
