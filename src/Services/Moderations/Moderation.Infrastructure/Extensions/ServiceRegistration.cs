namespace Moderation.Infrastructure.Extensions;

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

        services.AddDbContext<ModerationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpContextAccessor();

        services.AddScoped<IModerationUnitOfWork, ModerationUnitOfWork>();

        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IModerationActionRepository, ModerationActionRepository>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        return services;
    }
}
