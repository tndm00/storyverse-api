namespace Be.StoryVerse.ApiCommon.Middlewares;

/// <summary>
/// Reads the <c>X-Correlation-ID</c> header (the gateway Cloudflare Worker
/// already generates one for every request; a direct call bypassing the
/// gateway gets a fresh one here instead), makes it available to
/// <see cref="ResponseMetaDto"/> via <see cref="CorrelationContext"/>, echoes
/// it back on the response, and pushes it into the logger scope so every log
/// line for this request carries it — this is what lets a single request be
/// traced across services in Kibana.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(CorrelationConstants.HeaderName, out var existing)
            && !string.IsNullOrWhiteSpace(existing)
            ? existing.ToString()
            : Guid.NewGuid().ToString();

        CorrelationContext.CorrelationId = correlationId;
        context.Items[CorrelationConstants.HeaderName] = correlationId;
        context.Response.Headers[CorrelationConstants.HeaderName] = correlationId;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            [nameof(ResponseMetaDto.CorrelationId)] = correlationId
        }))
        {
            await _next(context);
        }
    }
}

/// <summary>
/// Registers <see cref="CorrelationIdMiddleware"/> in the request pipeline.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseStoryVerseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }
}
