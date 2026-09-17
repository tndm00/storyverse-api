namespace Moderation.Infrastructure.Extensions;

/// <summary>
/// Registers Infrastructure-layer services: DbContext, repositories, concrete
/// implementations of Application interfaces, and the typed HTTP clients for
/// service-to-service calls, per codebase-architecture-flow.md section 8.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>Registers the DbContext, repositories, current-user context, and downstream HTTP clients.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core / Npgsql DbContext.
        var connectionString = configuration.GetConnectionString(InfrastructureConstants.ConnectionStringName);

        services.AddDbContext<ModerationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpContextAccessor();

        // Application-facing interfaces bound to their Infrastructure implementations.
        services.AddScoped<IModerationUnitOfWork, ModerationUnitOfWork>();

        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IModerationActionRepository, ModerationActionRepository>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        // Forwards X-Correlation-ID on every outbound inter-service HTTP call
        // below, so one request traces across services in Kibana.
        services.AddTransient<CorrelationIdDelegatingHandler>();

        // Bind downstream service options and register their typed HTTP clients.
        services.Configure<ContentApiOptions>(configuration.GetSection(ContentApiOptions.SectionName));
        services.Configure<CommunityApiOptions>(configuration.GetSection(CommunityApiOptions.SectionName));
        services.Configure<AuthApiOptions>(configuration.GetSection(AuthApiOptions.SectionName));

        AddDownstreamClient<IContentModerationClient, ContentModerationClient, ContentApiOptions>(services);
        AddDownstreamClient<ICommunityModerationClient, CommunityModerationClient, CommunityApiOptions>(services);
        AddDownstreamClient<IUserDirectoryClient, UserDirectoryClient, AuthApiOptions>(services);

        return services;
    }

    /// <summary>Registers a typed HTTP client with its base address/timeout from options, correlation-id forwarding, and an optional dev-only certificate bypass.</summary>
    private static void AddDownstreamClient<TClient, TImplementation, TOptions>(IServiceCollection services)
        where TClient : class
        where TImplementation : class, TClient
        where TOptions : DownstreamApiOptions
    {
        services.AddHttpClient<TClient, TImplementation>((sp, client) =>
        {
            // Base address and timeout come from the bound downstream options.
            var options = sp.GetRequiredService<IOptions<TOptions>>().Value;
            if (!string.IsNullOrWhiteSpace(options.BaseUrl))
            {
                client.BaseAddress = new Uri(options.BaseUrl.EndsWith('/') ? options.BaseUrl : options.BaseUrl + "/");
            }

            client.Timeout = TimeSpan.FromSeconds(10);
        })
        .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
        .ConfigurePrimaryHttpMessageHandler(sp =>
        {
            // Optional, explicitly opt-in bypass of server certificate validation (non-production only).
            var handler = new System.Net.Http.HttpClientHandler();
            if (sp.GetRequiredService<IOptions<TOptions>>().Value.DangerousAcceptAnyServerCertificate)
            {
                handler.ServerCertificateCustomValidationCallback =
                    System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            }

            return handler;
        });
    }
}
