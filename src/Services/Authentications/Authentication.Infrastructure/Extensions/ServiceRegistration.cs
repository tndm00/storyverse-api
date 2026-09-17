namespace Authentication.Infrastructure.Extensions;

/// <summary>
/// Registers Infrastructure-layer services: DbContext, repositories, and
/// concrete implementations of Application interfaces, per
/// codebase-architecture-flow.md section 8 (DI registration pattern).
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Registers the Authentication service's DbContext, options, and repository/security
    /// implementations for the given <see cref="IServiceCollection"/>.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Wire up the EF Core context against the configured Postgres connection string.
        var connectionString = configuration.GetConnectionString(InfrastructureConstants.ConnectionStringName);

        services.AddDbContext<AuthenticationDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Bind strongly-typed options from configuration (JWT signing, Google OAuth).
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<GoogleAuthOptions>(configuration.GetSection(GoogleAuthOptions.SectionName));

        services.AddHttpContextAccessor();

        // Repository and security service implementations for Application-layer interfaces.
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IAuthorProfileRepository, AuthorProfileRepository>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();
        services.AddScoped<IUserSessionIssuer, UserSessionIssuer>();

        return services;
    }
}
