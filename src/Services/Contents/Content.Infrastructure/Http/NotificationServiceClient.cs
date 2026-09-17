using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Content.Infrastructure.Http;

/// <summary>
/// Typed <see cref="HttpClient"/> that creates notifications on the Notification
/// service. Best-effort by contract: it does not retry and callers are expected
/// to catch. A missing <see cref="NotificationApiOptions.ServiceToken"/> or
/// <see cref="NotificationApiOptions.BaseUrl"/> short-circuits to a no-op so a
/// half-configured environment never breaks approve/reject.
/// </summary>
public sealed class NotificationServiceClient : INotificationServiceClient
{
    private const string CreateNotificationPath = "v1/notifications";

    private readonly HttpClient _httpClient;
    private readonly NotificationApiOptions _options;
    private readonly ILogger<NotificationServiceClient> _logger;

    public NotificationServiceClient(
        HttpClient httpClient,
        IOptions<NotificationApiOptions> options,
        ILogger<NotificationServiceClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Sends a best-effort request to create a notification on the Notification service.
    /// No-ops (with a warning log) when the Notification API is not configured.
    /// </summary>
    public async Task SendAsync(
        long userId,
        NotificationKind kind,
        string title,
        string body,
        string refType,
        Guid? refId,
        CancellationToken cancellationToken)
    {
        // Notification API not configured: short-circuit rather than fail the caller's flow.
        if (string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.ServiceToken))
        {
            _logger.LogWarning(
                "Notification API not configured (BaseUrl/ServiceToken missing); skipped {Kind} notification for user {UserId}.",
                kind, userId);
            return;
        }

        var payload = new
        {
            userId,
            type = kind.ToString(),
            title,
            body,
            refType,
            refId
        };

        // Post the notification, authenticating with the shared service token.
        using var request = new HttpRequestMessage(HttpMethod.Post, CreateNotificationPath)
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add(ServiceAuthConstants.HeaderName, _options.ServiceToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
