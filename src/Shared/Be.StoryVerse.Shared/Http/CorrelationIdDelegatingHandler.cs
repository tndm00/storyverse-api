namespace Be.StoryVerse.Shared.Http;

/// <summary>
/// Forwards the ambient <see cref="CorrelationContext.CorrelationId"/> as the
/// <c>X-Correlation-ID</c> header on every outbound inter-service HTTP call.
/// CorrelationIdMiddleware on the receiving service reads that header instead
/// of minting a new ID, so one CorrelationId traces a request across every
/// service it touches instead of resetting at each hop.
/// </summary>
public sealed class CorrelationIdDelegatingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = CorrelationContext.CorrelationId;
        if (!string.IsNullOrEmpty(correlationId) && !request.Headers.Contains(CorrelationConstants.HeaderName))
        {
            request.Headers.Add(CorrelationConstants.HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
