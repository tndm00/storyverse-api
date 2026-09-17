namespace Be.StoryVerse.ApiCommon.Logging;

/// <summary>
/// Stamps every log event with the ambient correlation ID
/// (<see cref="CorrelationContext"/>, set by CorrelationIdMiddleware) so it
/// becomes a real, filterable JSON field once shipped to Elasticsearch,
/// instead of only living in the HTTP response envelope.
/// </summary>
public sealed class CorrelationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = CorrelationContext.CorrelationId;
        if (string.IsNullOrEmpty(correlationId))
        {
            return;
        }

        logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("CorrelationId", correlationId));
    }
}
