namespace Community.Infrastructure.Context;

/// <summary>
/// Design-time factory so <c>dotnet ef</c> can build <see cref="CommunityDbContext"/>
/// without booting the API host. Never used at runtime. The connection string is
/// only used to resolve the Npgsql provider for migration scaffolding; override it
/// with <c>COMMUNITY_DB_CONNECTION</c> when generating provider-specific SQL.
/// </summary>
public sealed class CommunityDbContextFactory : IDesignTimeDbContextFactory<CommunityDbContext>
{
    private const string FallbackConnectionString =
        "Host=localhost;Port=5432;Database=storyverse_community;Username=postgres;Password=postgres";

    /// <summary>Builds a <see cref="CommunityDbContext"/> for design-time tooling (e.g. <c>dotnet ef</c> migrations).</summary>
    public CommunityDbContext CreateDbContext(string[] args)
    {
        // Allow overriding the connection string via env var for provider-specific SQL generation.
        var connectionString = Environment.GetEnvironmentVariable("COMMUNITY_DB_CONNECTION") ?? FallbackConnectionString;

        var options = new DbContextOptionsBuilder<CommunityDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new CommunityDbContext(options);
    }
}
