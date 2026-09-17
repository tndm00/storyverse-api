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

    private readonly HttpClient _httpClient;
    private readonly ContentApiOptions _options;
    private readonly ILogger<ContentCommentCountSyncClient> _logger;

    /// <summary>Creates the client with its injected typed <see cref="HttpClient"/>, options, and logger.</summary>
    public ContentCommentCountSyncClient(
        HttpClient httpClient,
        IOptions<ContentApiOptions> options,
        ILogger<ContentCommentCountSyncClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Pushes the recomputed visible comment count for a chapter to the Content
    /// service. Best-effort: never throws, since the local comment write already committed.
    /// </summary>
    public async Task SyncCommentCountAsync(Guid chapterId, int commentCount, CancellationToken cancellationToken)
    {
        // No Content API configured: skip the sync silently (logged as a warning).
        if (!IsConfigured())
        {
            _logger.LogWarning(
                "Content API not configured (Services:ContentApi); chapter {ChapterId} comment count was not synced.",
                chapterId);
            return;
        }

        try
        {
            // Push the new count to the internal Content endpoint with the service token.
            using var request = new HttpRequestMessage(HttpMethod.Post, $"v1/chapters/{chapterId}/comment-count")
            {
                Content = JsonContent.Create(new { count = commentCount })
            };
            request.Headers.Add(ServiceAuthConstants.HeaderName, _options.ServiceToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            // Network/response failure: log and swallow, leaving the count stale.
            _logger.LogWarning(
                ex, "Comment count sync to Content failed for chapter {ChapterId}; Chapter.CommentCount stays stale.",
                chapterId);
        }
    }

    /// <summary>Whether both the base URL and service token are set, so the call can be made.</summary>
    private bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(_options.BaseUrl) && !string.IsNullOrWhiteSpace(_options.ServiceToken);
}
