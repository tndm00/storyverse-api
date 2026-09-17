using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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
    public const string HeaderName = "X-Correlation-ID";

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var correlationId = CorrelationContext.CorrelationId;
        if (!string.IsNullOrEmpty(correlationId) && !request.Headers.Contains(HeaderName))
        {
            request.Headers.Add(HeaderName, correlationId);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
