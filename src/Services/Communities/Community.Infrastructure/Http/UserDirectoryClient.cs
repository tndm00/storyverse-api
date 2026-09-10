using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Community.Infrastructure.Http;

/// <summary>
/// Request-scoped typed <see cref="HttpClient"/> for the Authentication service's
/// internal <c>GET /v1/auth/internal/users?ids=</c> batch lookup. Caches resolved
/// names for the lifetime of the request (the DI scope), so a page of comments
/// that share authors costs a single call. Every failure path is swallowed and
/// logged — the caller renders the numeric-id fallback.
/// </summary>
public sealed class UserDirectoryClient : IUserDirectoryClient
{
    private const string ServiceTokenHeader = "X-Service-Token";

    private readonly HttpClient _httpClient;
    private readonly AuthApiOptions _options;
    private readonly ILogger<UserDirectoryClient> _logger;
    private readonly Dictionary<long, string> _cache = new();

    public UserDirectoryClient(
        HttpClient httpClient,
        IOptions<AuthApiOptions> options,
        ILogger<UserDirectoryClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyDictionary<long, string>> GetDisplayNamesAsync(
        IEnumerable<long> userIds, CancellationToken cancellationToken)
    {
        var requested = userIds.Where(id => id > 0).Distinct().ToArray();
        var missing = requested.Where(id => !_cache.ContainsKey(id)).ToArray();

        if (missing.Length > 0 && IsConfigured())
        {
            await FetchAsync(missing, cancellationToken);
        }
        else if (missing.Length > 0)
        {
            _logger.LogWarning("Auth API not configured (Services:AuthApi); comment/rating names fall back to id.");
        }

        return requested
            .Where(id => _cache.ContainsKey(id))
            .ToDictionary(id => id, id => _cache[id]);
    }

    private bool IsConfigured() =>
        !string.IsNullOrWhiteSpace(_options.BaseUrl) && !string.IsNullOrWhiteSpace(_options.ServiceToken);

    private async Task FetchAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken)
    {
        try
        {
            var query = string.Join(',', ids);
            using var request = new HttpRequestMessage(HttpMethod.Get, $"v1/auth/internal/users?ids={query}");
            request.Headers.Add(ServiceTokenHeader, _options.ServiceToken);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var envelope = await response.Content.ReadFromJsonAsync<Envelope>(cancellationToken);
            foreach (var entry in envelope?.Data ?? new List<Entry>())
            {
                if (entry.UserId > 0 && !string.IsNullOrWhiteSpace(entry.DisplayName))
                {
                    _cache[entry.UserId] = entry.DisplayName;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Display-name lookup failed for {Count} user ids; falling back to id.", ids.Count);
        }
    }

    private sealed class Envelope
    {
        public List<Entry> Data { get; set; }
    }

    private sealed class Entry
    {
        public long UserId { get; set; }

        public string DisplayName { get; set; }
    }
}
