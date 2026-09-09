namespace Notification.Api.Extensions;

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
                // Accept/emit enum names (e.g. "NewChapter") rather than ordinals,
                // so request/response contracts stay business-readable and never
                // leak enum numbering (api-guidelines.md section 10-11).
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddStoryVerseSwagger(ApiConstants.SwaggerTitle);

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                // Keep JWT claim types verbatim ("sub", ...) so the Application
                // layer can read them by their registered names.
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

        services.AddAuthorization();

        services.AddStoryVerseCors(configuration);

        return services;
    }
}
