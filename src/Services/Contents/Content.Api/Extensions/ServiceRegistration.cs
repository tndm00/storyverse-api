using Content.Api.BackgroundServices;
using Content.Api.Seed;

namespace Content.Api.Extensions;

/// <summary>
/// API-only startup wiring: JWT Bearer authentication, Swagger, and MVC/controllers,
/// per codebase-architecture-flow.md section 8 and auth-guidelines.md section 2.
/// </summary>
public static class ServiceRegistration
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                // Accept/emit enum names (e.g. "Draft", "Translated") rather than
                // ordinals, so request/response contracts stay business-readable
                // and never leak enum numbering (api-guidelines.md section 10-11).
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddStoryVerseSwagger(ApiConstants.SwaggerTitle);

        // Shared secret for trusted service-to-service callers of internal endpoints
        // (see ServiceOrUserAuthorizeAttribute): Moderation applying Hide/Remove.
        services.Configure<Content.Application.Options.ServiceAuthOptions>(
            configuration.GetSection(Content.Application.Options.ServiceAuthOptions.SectionName));

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep JWT claim types verbatim ("sub", "author_id", ...) so the
                // Application layer can read them by their registered names.
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
        // StoryVersePermissions entry, backed by the shared role->permission map.
        services.AddStoryVersePermissions();

        services.AddStoryVerseCors(configuration);

        // Background loop that auto-publishes Scheduled chapters when due.
        services.Configure<Content.Application.Options.ChapterPublishingOptions>(
            configuration.GetSection(Content.Application.Options.ChapterPublishingOptions.SectionName));
        services.AddHostedService<ScheduledChapterPublisher>();

        // Dev-only seeders. GenreSeeder must run first — DemoContentSeeder looks
        // its genres up by slug. Hosted services start in registration order.
        services.AddHostedService<GenreSeeder>();
        services.AddHostedService<DemoContentSeeder>();

        return services;
    }
}
