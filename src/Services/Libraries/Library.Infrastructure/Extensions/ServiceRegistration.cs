namespace Library.Infrastructure.Extensions;

/// <summary>
/// Registers Infrastructure-layer services: DbContext, repositories, and
/// concrete implementations of Application interfaces, per
/// codebase-architecture-flow.md section 8 (DI registration pattern).
/// </summary>
public static class ServiceRegistration
{
    /// <summary>Wires up the Library service's DbContext, unit of work, repositories and current-user context.</summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext bound to the Library database via Npgsql.
        var connectionString = configuration.GetConnectionString(InfrastructureConstants.ConnectionStringName);

        services.AddDbContext<LibraryDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddHttpContextAccessor();

        services.AddScoped<ILibraryUnitOfWork, LibraryUnitOfWork>();

        // Repository and current-user implementations of Application-layer interfaces.
        services.AddScoped<ILibraryEntryRepository, LibraryEntryRepository>();
        services.AddScoped<IReadingProgressRepository, ReadingProgressRepository>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        return services;
    }
}
