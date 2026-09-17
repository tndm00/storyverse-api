namespace Be.StoryVerse.Shared.Constants;

/// <summary>
/// Correlation ID wire format, shared by CorrelationIdMiddleware
/// (Be.StoryVerse.ApiCommon, sets it per inbound request) and
/// CorrelationIdDelegatingHandler (Be.StoryVerse.Shared, forwards it on
/// outbound inter-service calls) so both sides agree on the exact header
/// name without duplicating the literal.
/// </summary>
public static class CorrelationConstants
{
    public const string HeaderName = "X-Correlation-ID";
}
