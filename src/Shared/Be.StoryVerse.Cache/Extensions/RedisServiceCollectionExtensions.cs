namespace Be.StoryVerse.Cache.Extensions;

/// <summary>Registers the shared Redis connection for any service that needs it.</summary>
public static class RedisServiceCollectionExtensions
{
    /// <summary>
    /// Binds <see cref="RedisOptions"/> from the <c>Redis</c> section and registers a lazily created
    /// <see cref="IConnectionMultiplexer"/>. Lazy, so no connection is even attempted while
    /// <see cref="RedisOptions.Enabled"/> is false.
    /// </summary>
    public static IServiceCollection AddRedisConnection(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

        services.AddSingleton(sp => new Lazy<IConnectionMultiplexer>(() =>
        {
            var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            var redisConfiguration = ConfigurationOptions.Parse(options.ConnectionString);

            // Never crash startup because Redis is down; fail commands fast instead of queueing them.
            redisConfiguration.AbortOnConnectFail = false;
            redisConfiguration.BacklogPolicy = BacklogPolicy.FailFast;
            redisConfiguration.ConnectTimeout = options.CommandTimeoutMs;
            redisConfiguration.SyncTimeout = options.CommandTimeoutMs;
            redisConfiguration.AsyncTimeout = options.CommandTimeoutMs;

            return ConnectionMultiplexer.Connect(redisConfiguration);
        }));

        return services;
    }

    /// <summary>
    /// Registers <see cref="RedisViewStore"/> as the view tracker, the pending-count buffer and the
    /// statistics reader. The calling service must also register an <see cref="IViewCountFallback"/>.
    /// </summary>
    public static IServiceCollection AddRedisViewTracking(this IServiceCollection services)
    {
        services.AddScoped<RedisViewStore>();
        services.AddScoped<IViewTracker>(sp => sp.GetRequiredService<RedisViewStore>());
        services.AddScoped<IViewCountBuffer>(sp => sp.GetRequiredService<RedisViewStore>());
        services.AddScoped<IViewStatsReader>(sp => sp.GetRequiredService<RedisViewStore>());

        return services;
    }
}
