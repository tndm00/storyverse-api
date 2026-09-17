using Be.StoryVerse.Shared.Http;
using Microsoft.Extensions.Options;

namespace Community.Infrastructure.Extensions;

/// <summary>
/// Registers Infrastructure-layer services: DbContext, repositories, and
/// concrete implementations of Application interfaces, per
/// codebase-architecture-flow.md section 8 (DI registration pattern).
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(InfrastructureConstants.ConnectionStringName);

        services.AddDbContext<CommunityDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpContextAccessor();

        services.AddScoped<ICommunityUnitOfWork, CommunityUnitOfWork>();

        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IRatingRepository, RatingRepository>();
        services.AddScoped<IVoteRepository, VoteRepository>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        // Forwards X-Correlation-ID on every outbound inter-service HTTP call
        // below, so one request traces across services in Kibana.
        services.AddTransient<CorrelationIdDelegatingHandler>();

        // Outbound HTTP to the Authentication service: resolves author/rater user
        // ids to display names for comment and rating listings. Request-scoped so
        // the per-request name cache lives exactly as long as the request.
        services.Configure<AuthApiOptions>(configuration.GetSection(AuthApiOptions.SectionName));
        services.AddHttpClient<IUserDirectoryClient, UserDirectoryClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<AuthApiOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/");
            }

            client.Timeout = TimeSpan.FromSeconds(5);
        })
        .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
        .ConfigurePrimaryHttpMessageHandler(sp =>
        {
            var handler = new System.Net.Http.HttpClientHandler();
            if (sp.GetRequiredService<IOptions<AuthApiOptions>>().Value.DangerousAcceptAnyServerCertificate)
            {
                handler.ServerCertificateCustomValidationCallback =
                    System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // Outbound HTTP to the Content service: pushes the recomputed rating
        // aggregate after a rating write commits, so Story.RatingAvg/RatingCount
        // stay in sync for story listings. Best-effort: see ContentRatingSyncClient.
        services.Configure<ContentApiOptions>(configuration.GetSection(ContentApiOptions.SectionName));
        services.AddHttpClient<IContentRatingSyncClient, ContentRatingSyncClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ContentApiOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/");
            }

            client.Timeout = TimeSpan.FromSeconds(5);
        })
        .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
        .ConfigurePrimaryHttpMessageHandler(sp =>
        {
            var handler = new System.Net.Http.HttpClientHandler();
            if (sp.GetRequiredService<IOptions<ContentApiOptions>>().Value.DangerousAcceptAnyServerCertificate)
            {
                handler.ServerCertificateCustomValidationCallback =
                    System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // Outbound HTTP to the Content service: pushes the recomputed visible
        // comment count after a comment write commits, so Chapter.CommentCount
        // stays in sync for story sorting/listings. Best-effort: see
        // ContentCommentCountSyncClient. Same Services:ContentApi config as above.
        services.AddHttpClient<IContentCommentCountSyncClient, ContentCommentCountSyncClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ContentApiOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/");
            }

            client.Timeout = TimeSpan.FromSeconds(5);
        })
        .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
        .ConfigurePrimaryHttpMessageHandler(sp =>
        {
            var handler = new System.Net.Http.HttpClientHandler();
            if (sp.GetRequiredService<IOptions<ContentApiOptions>>().Value.DangerousAcceptAnyServerCertificate)
            {
                handler.ServerCertificateCustomValidationCallback =
                    System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        // Outbound HTTP to the Content service: batch-resolves chapter ->
        // story context to enrich the cross-platform recent-comments feed.
        // Best-effort: see ContentChapterContextClient. Same config as above.
        services.AddHttpClient<IContentChapterContextClient, ContentChapterContextClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ContentApiOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/");
            }

            client.Timeout = TimeSpan.FromSeconds(5);
        })
        .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
        .ConfigurePrimaryHttpMessageHandler(sp =>
        {
            var handler = new System.Net.Http.HttpClientHandler();
            if (sp.GetRequiredService<IOptions<ContentApiOptions>>().Value.DangerousAcceptAnyServerCertificate)
            {
                handler.ServerCertificateCustomValidationCallback =
                    System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        })
        .SetHandlerLifetime(TimeSpan.FromMinutes(5));

        return services;
    }
}
