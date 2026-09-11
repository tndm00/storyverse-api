using System.Net.Http;
using System.Net.Http.Json;
using Community.Application.Dtos;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Community.Infrastructure.Http;

/// <summary>
/// Request-scoped typed <see cref="HttpClient"/> for the Content service's
/// internal <c>GET /v1/chapters/internal/context?ids=</c> batch lookup. Every
/// failure path (not configured, non-success response, network error) is
/// swallowed and logged — the caller simply gets an empty dictionary and
/// renders the affected items without story/chapter context. Mirrors
/// <see cref="UserDirectoryClient"/>.
/// </summary>
public sealed class ContentChapterContextClient : IContentChapterContextClient
{
    private const string ServiceTokenHeader = "X-Service-Token";

    private readonly HttpClient _httpClient;
    private readonly ContentApiOptions _options;
    private readonly ILogger<ContentChapterContextClient> _logger;

    public ContentChapterContextClient(
        HttpClient httpClient,
        IOptions<ContentApiOptions> options,
        ILogger<ContentChapterContextClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<Guid, ChapterContextDto>> GetContextAsync(
        IEnumerable<Guid> chapterIds, CancellationToken cancellationToken)
    {
        var ids = chapterIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return new Dictionary<Guid, ChapterContextDto>();
        }

        if (!IsConfigured())
        {
            _logger.LogWarning(
                "Content API not configured (Services:ContentApi); chapter context lookup skipped for {Count} chapter(s).",
                ids.Length);
            return new Dictionary<Guid, ChapterContextDto>();
        }

        try
        {
            var query = string.Join(',', ids);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"v1/chapters/internal/context?ids={query}");
            request.Headers.Add(ServiceTokenHeader, _options.ServiceToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);
            return (envelope?.Data ?? new List<Entry>())
                .Where(e => e.ChapterId != Guid.Empty)
                .ToDictionary(
                    e => e.ChapterId,
                    e => new ChapterContextDto
                    {
                        ChapterId = e.ChapterId,
                        ChapterTitle = e.ChapterTitle,
                        StoryId = e.StoryId,
                        StorySlug = e.StorySlug,
                        StoryTitle = e.StoryTitle
                    });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex, "Chapter context lookup failed for {Count} chapter id(s); story/chapter fields will be null.",
                ids.Length);
            return new Dictionary<Guid, ChapterContextDto>();
        }
    }

    private bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(_options.BaseUrl) && !string.IsNullOrWhiteSpace(_options.ServiceToken);

    private sealed class Envelope
    {
        public List<Entry> Data { get; set; }
    }

    private sealed class Entry
    {
        public Guid ChapterId { get; set; }

        public string ChapterTitle { get; set; }

        public Guid StoryId { get; set; }

        public string StorySlug { get; set; }

        public string StoryTitle { get; set; }
    }
}
