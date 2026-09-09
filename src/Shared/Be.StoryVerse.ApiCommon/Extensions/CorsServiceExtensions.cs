using Microsoft.Extensions.Configuration;

namespace Be.StoryVerse.ApiCommon.Extensions;

/// <summary>
/// Shared CORS wiring so every API host lets the browser client (story-fe) call
/// it directly. Allowed origins come from <c>Cors:AllowedOrigins</c> in config;
/// when that is empty the local dev origins are used so a fresh checkout works
/// with no extra setup. There is no API gateway in front of the services, so
/// each host must send its own CORS headers.
/// </summary>
public static class CorsServiceExtensions
{
    public const string PolicyName = "StoryVerseClient";

    private static readonly string[] DefaultDevOrigins =
    {
        "http://localhost:5173",
        "http://127.0.0.1:5173",
        "https://localhost:5173"
    };

    public static IServiceCollection AddStoryVerseCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var configured = configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        var origins = configured is { Length: > 0 } ? configured : DefaultDevOrigins;

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy
                    .WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Applies the shared CORS policy. Call after <c>UseHttpsRedirection</c> and
    /// before <c>UseAuthentication</c> / <c>UseAuthorization</c>.
    /// </summary>
    public static IApplicationBuilder UseStoryVerseCors(this IApplicationBuilder app)
    {
        return app.UseCors(PolicyName);
    }
}
