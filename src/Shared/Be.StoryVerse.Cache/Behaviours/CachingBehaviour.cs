namespace Be.StoryVerse.Cache.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that would short-circuit a query with a cached
/// response when the request opts in. This is a minimal stub demonstrating the
/// pattern referenced by code-standard.md section 13 (ApiCommon/Cache Behaviours);
/// requests must implement <see cref="ICacheableQuery"/> to participate.
/// </summary>
public sealed class CachingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehaviour<TRequest, TResponse>> _logger;

    public CachingBehaviour(ICacheService cacheService, ILogger<CachingBehaviour<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
        {
            return await next();
        }

        var cached = await _cacheService.GetAsync<TResponse>(cacheable.CacheKey, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var response = await next();

        await _cacheService.SetAsync(cacheable.CacheKey, response, cacheable.AbsoluteExpiration, cancellationToken);

        return response;
    }
}

/// <summary>
/// Implemented by queries that want to participate in <see cref="CachingBehaviour{TRequest,TResponse}"/>.
/// </summary>
public interface ICacheableQuery
{
    string CacheKey { get; }

    TimeSpan? AbsoluteExpiration { get; }
}
