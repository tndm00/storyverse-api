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

        return services;
    }
}
