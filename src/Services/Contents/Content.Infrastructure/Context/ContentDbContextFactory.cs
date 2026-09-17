namespace Content.Infrastructure.Context;

/// <summary>
/// Design-time factory so <c>dotnet ef</c> can build <see cref="ContentDbContext"/>
/// without booting the API host. Never used at runtime. The connection string is
/// only used to resolve the Npgsql provider for migration scaffolding; override it
/// with <c>CONTENT_DB_CONNECTION</c> when generating provider-specific SQL.
/// </summary>
public sealed class ContentDbContextFactory : IDesignTimeDbContextFactory<ContentDbContext>
{
    private const string FallbackConnectionString =
        "Host=localhost;Port=5432;Database=storyverse_content;Username=postgres;Password=postgres";

    /// <summary>
    /// Builds a <see cref="ContentDbContext"/> for design-time tooling (e.g. <c>dotnet ef</c>),
    /// using the <c>CONTENT_DB_CONNECTION</c> environment variable or a local fallback connection string.
    /// </summary>
    public ContentDbContext CreateDbContext(string[] args)
    {
        // Prefer the env-provided connection string; fall back to a local dev default.
        var connectionString = Environment.GetEnvironmentVariable("CONTENT_DB_CONNECTION") ?? FallbackConnectionString;

        var options = new DbContextOptionsBuilder<ContentDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ContentDbContext(options);
    }
}
