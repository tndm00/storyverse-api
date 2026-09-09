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
        services.AddScoped<IVolumeRepository, VolumeRepository>();
        services.AddScoped<IGenreRepository, GenreRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<ICurrentAuthorContext, CurrentAuthorContext>();

        return services;
    }
}
