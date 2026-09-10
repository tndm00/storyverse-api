using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Moderation.Infrastructure.Http;

/// <summary>Shared JSON envelope for the internal lookup endpoints (<c>ResponseDto&lt;T&gt;</c>).</summary>
internal sealed class ListEnvelope<T>
{
    public List<T> Data { get; set; }
}

internal sealed class IdTitle
{
    public Guid Id { get; set; }

    public string Title { get; set; }

    public string Excerpt { get; set; }
}

internal sealed class UserNameEntry
{
    public long UserId { get; set; }

    public string DisplayName { get; set; }
}

/// <summary>Typed client for the Content service (Hide/Remove application + title lookup).</summary>
public sealed class ContentModerationClient : IContentModerationClient
{
    private const string ServiceTokenHeader = "X-Service-Token";

    private readonly HttpClient _httpClient;
    private readonly ContentApiOptions _options;
    private readonly ILogger<ContentModerationClient> _logger;

    public ContentModerationClient(
        HttpClient httpClient, IOptions<ContentApiOptions> options, ILogger<ContentModerationClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public Task SetStoryVisibilityAsync(Guid storyId, bool hidden, string reason, CancellationToken cancellationToken) =>
        PostVisibilityAsync($"v1/stories/{storyId}/moderation-visibility", hidden, reason, cancellationToken);

    public Task SetChapterVisibilityAsync(Guid chapterId, bool hidden, string reason, CancellationToken cancellationToken) =>
        PostVisibilityAsync($"v1/chapters/{chapterId}/moderation-visibility", hidden, reason, cancellationToken);

    private async Task PostVisibilityAsync(string path, bool hidden, string reason, CancellationToken cancellationToken)
    {
        if (!_options.IsConfigured)
        {
            // A moderation decision must take effect; a missing config is a hard error here.
            throw new InvalidOperationException(
                "Services:ContentApi is not configured; cannot apply the moderation decision to content.");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(new { hidden, reason })
        };
        request.Headers.Add(ServiceTokenHeader, _options.ServiceToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetTitlesAsync(
        ModerationTargetType targetType, IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.Where(id => id != Guid.Empty).Distinct().ToArray();
        if (idList.Length == 0 || !_options.IsConfigured)
        {
            return new Dictionary<Guid, string>();
        }

        var segment = targetType == ModerationTargetType.Chapter ? "chapters" : "stories";
        var path = $"v1/{segment}/internal/titles?ids={string.Join(',', idList)}";

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Add(ServiceTokenHeader, _options.ServiceToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<ListEnvelope<IdTitle>>(cancellationToken);
            return (envelope?.Data ?? new List<IdTitle>())
                .Where(x => x.Id != Guid.Empty && !string.IsNullOrWhiteSpace(x.Title))
                .ToDictionary(x => x.Id, x => x.Title);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Content title lookup failed for {Count} {TargetType} ids.", idList.Length, targetType);
            return new Dictionary<Guid, string>();
        }
    }
}

/// <summary>Typed client for the Community service (comment Hide application + excerpt lookup).</summary>
public sealed class CommunityModerationClient : ICommunityModerationClient
{
    private const string ServiceTokenHeader = "X-Service-Token";

    private readonly HttpClient _httpClient;
    private readonly CommunityApiOptions _options;
    private readonly ILogger<CommunityModerationClient> _logger;

    public CommunityModerationClient(
        HttpClient httpClient, IOptions<CommunityApiOptions> options, ILogger<CommunityModerationClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SetCommentVisibilityAsync(
        Guid commentId, bool hidden, string reason, CancellationToken cancellationToken)
    {
        if (!_options.IsConfigured)
        {
            throw new InvalidOperationException(
                "Services:CommunityApi is not configured; cannot apply the moderation decision to the comment.");
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Post, $"v1/comments/{commentId}/moderation-visibility")
        {
            Content = JsonContent.Create(new { hidden, reason })
        };
        request.Headers.Add(ServiceTokenHeader, _options.ServiceToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IReadOnlyDictionary<Guid, string>> GetCommentExcerptsAsync(
        IEnumerable<Guid> ids, CancellationToken cancellationToken)
    {
        var idList = ids.Where(id => id != Guid.Empty).Distinct().ToArray();
        if (idList.Length == 0 || !_options.IsConfigured)
        {
            return new Dictionary<Guid, string>();
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, $"v1/comments/internal/excerpts?ids={string.Join(',', idList)}");
            request.Headers.Add(ServiceTokenHeader, _options.ServiceToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<ListEnvelope<IdTitle>>(cancellationToken);
            return (envelope?.Data ?? new List<IdTitle>())
                .Where(x => x.Id != Guid.Empty && !string.IsNullOrWhiteSpace(x.Excerpt))
                .ToDictionary(x => x.Id, x => x.Excerpt);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Community comment excerpt lookup failed for {Count} ids.", idList.Length);
            return new Dictionary<Guid, string>();
        }
    }
}

/// <summary>Typed client for the Authentication service's batch display-name lookup.</summary>
public sealed class UserDirectoryClient : IUserDirectoryClient
{
    private const string ServiceTokenHeader = "X-Service-Token";

    private readonly HttpClient _httpClient;
    private readonly AuthApiOptions _options;
    private readonly ILogger<UserDirectoryClient> _logger;

    public UserDirectoryClient(
        HttpClient httpClient, IOptions<AuthApiOptions> options, ILogger<UserDirectoryClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<long, string>> GetDisplayNamesAsync(
        IEnumerable<long> userIds, CancellationToken cancellationToken)
    {
        var idList = userIds.Where(id => id > 0).Distinct().ToArray();
        if (idList.Length == 0 || !_options.IsConfigured)
        {
            return new Dictionary<long, string>();
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get, $"v1/auth/internal/users?ids={string.Join(',', idList)}");
            request.Headers.Add(ServiceTokenHeader, _options.ServiceToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<ListEnvelope<UserNameEntry>>(cancellationToken);
            return (envelope?.Data ?? new List<UserNameEntry>())
                .Where(x => x.UserId > 0 && !string.IsNullOrWhiteSpace(x.DisplayName))
                .ToDictionary(x => x.UserId, x => x.DisplayName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Reporter display-name lookup failed for {Count} ids.", idList.Length);
            return new Dictionary<long, string>();
        }
    }
}
