namespace Be.StoryVerse.ApiCommon.Extensions;

/// <summary>
/// Shared Swagger wiring so every API host configures documentation and the
/// JWT bearer security scheme consistently, per code-standard.md section 7.5
/// (ApiCommon owns Swagger extensions).
/// </summary>
public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddStoryVerseSwagger(
        this IServiceCollection services,
        string title,
        string version = ApiCommonConstants.DefaultApiVersion)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(version, new OpenApiInfo { Title = title, Version = version });

            var securityScheme = new OpenApiSecurityScheme
            {
                Name = ApiCommonConstants.AuthorizationHeaderName,
                Type = SecuritySchemeType.Http,
                Scheme = ApiCommonConstants.BearerScheme,
                BearerFormat = ApiCommonConstants.JwtBearerFormat,
                In = ParameterLocation.Header,
                Description = ApiCommonConstants.BearerTokenDescription
            };

            options.AddSecurityDefinition(ApiCommonConstants.BearerScheme, securityScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = ApiCommonConstants.BearerScheme }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static WebApplication UseStoryVerseSwagger(this WebApplication app, string version = ApiCommonConstants.DefaultApiVersion)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"{app.Environment.ApplicationName} {version}");
        });

        return app;
    }
}
