namespace Authentication.Api.Extensions;

/// <summary>
/// API-only startup wiring: JWT Bearer authentication, Swagger, and MVC/controllers,
/// per codebase-architecture-flow.md section 8 and auth-guidelines.md section 2.
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        services.AddStoryVerseSwagger(ApiConstants.SwaggerTitle);

        // Shared secret for trusted service-to-service callers of internal endpoints
        // (see ServiceOrUserAuthorizeAttribute).
        services.Configure<ServiceAuthOptions>(configuration.GetSection(ServiceAuthOptions.SectionName));

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep JWT claim types verbatim ("sub", "jti", "author_id", ...); the
                // Application layer reads them by their registered names, not the legacy
                // SOAP-schema URIs the default inbound mapper would rewrite them to.
                options.MapInboundClaims = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudiences = jwtOptions.Audiences,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        // Permission-based authorization: one "perm:<permission>" policy per
        // StoryVersePermissions entry (e.g. users.manage guards admin role grants).
        services.AddStoryVersePermissions();

        // Dev bootstrap: promote configured emails to PlatformAdmin once they
        // register (Seed:PlatformAdminEmails). No-op when the list is empty.
        services.AddHostedService<PlatformAdminSeeder>();

        services.AddStoryVerseCors(configuration);

        return services;
    }
}
