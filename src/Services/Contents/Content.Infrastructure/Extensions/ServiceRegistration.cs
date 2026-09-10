using Microsoft.Extensions.Options;

namespace Content.Infrastructure.Extensions;

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

        services.AddDbContext<ContentDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpContextAccessor();

        services.AddScoped<IContentUnitOfWork, ContentUnitOfWork>();

        services.AddScoped<IStoryRepository, StoryRepository>();
        services.AddScoped<IChapterRepository, ChapterRepository>();
        services.AddScoped<IChapterReviewActionRepository, ChapterReviewActionRepository>();
        services.AddScoped<IVolumeRepository, VolumeRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ICurrentAuthorContext, CurrentAuthorContext>();

        // Outbound HTTP to the Notification service (stand-in for an event bus).
        services.Configure<NotificationApiOptions>(configuration.GetSection(NotificationApiOptions.SectionName));
        services.AddHttpClient<INotificationServiceClient, NotificationServiceClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<NotificationApiOptions>>().Value;
            SetBaseAddress(client, options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        })
        .ConfigurePrimaryHttpMessageHandler(sp =>
            BuildHandler(sp.GetRequiredService<IOptions<NotificationApiOptions>>().Value.DangerousAcceptAnyServerCertificate));

        // Outbound HTTP to the Authentication service: resolves a story's
        // AuthorProfile id to the author's real user id for notifications.
        services.Configure<AuthApiOptions>(configuration.GetSection(AuthApiOptions.SectionName));
        services.AddHttpClient<IAuthorDirectoryClient, AuthDirectoryClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<AuthApiOptions>>().Value;
            SetBaseAddress(client, options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        })
        .ConfigurePrimaryHttpMessageHandler(sp =>
            BuildHandler(sp.GetRequiredService<IOptions<AuthApiOptions>>().Value.DangerousAcceptAnyServerCertificate));

        return services;
    }

    private static void SetBaseAddress(System.Net.Http.HttpClient client, string baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return;
        }

        client.BaseAddress = new Uri(baseUrl.EndsWith('/') ? baseUrl : baseUrl + "/");
    }

    private static System.Net.Http.HttpMessageHandler BuildHandler(bool acceptAnyServerCertificate)
    {
        var handler = new System.Net.Http.HttpClientHandler();
        if (acceptAnyServerCertificate)
        {
            handler.ServerCertificateCustomValidationCallback =
                System.Net.Http.HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        return handler;
    }
}
