using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Content.Infrastructure.Http;

/// <summary>
/// Typed <see cref="HttpClient"/> for the Authentication service's internal
/// AuthorProfile-&gt;User lookup. A missing <see cref="AuthApiOptions.BaseUrl"/>
/// or <see cref="AuthApiOptions.ServiceToken"/> short-circuits to
/// <c>null</c> (no recipient) rather than throwing, so a half-configured
/// environment never breaks approve/reject.
/// </summary>
public sealed class AuthDirectoryClient : IAuthorDirectoryClient
{
    /// <summary>Agreed with Authentication.Api (<c>ApiConstants.ServiceTokenHeader</c>).</summary>
    private const string ServiceTokenHeader = "X-Service-Token";

    private readonly HttpClient _httpClient;
    private readonly AuthApiOptions _options;
    private readonly ILogger<AuthDirectoryClient> _logger;

    public AuthDirectoryClient(
        HttpClient httpClient,
        IOptions<AuthApiOptions> options,
        ILogger<AuthDirectoryClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<long?> GetAuthorUserIdAsync(long authorProfileId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.ServiceToken))
        {
            _logger.LogWarning(
                "Auth API not configured (BaseUrl/ServiceToken missing); cannot resolve user id for author profile {AuthorProfileId}.",
                authorProfileId);
            return null;
        }

        using var request = new HttpRequestMessage(
            HttpMethod.Get, $"v1/auth/internal/author-profiles/{authorProfileId}");
        request.Headers.Add(ServiceTokenHeader, _options.ServiceToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<LookupEnvelope>(cancellationToken);
        return envelope?.Data?.UserId;
    }

    private sealed class LookupEnvelope
    {
        public LookupData Data { get; set; }
    }

    private sealed class LookupData
    {
        public long AuthorProfileId { get; set; }

        public long UserId { get; set; }
    }
}
