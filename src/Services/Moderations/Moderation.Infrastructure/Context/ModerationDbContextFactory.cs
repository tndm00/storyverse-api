namespace Moderation.Infrastructure.Context;

/// <summary>
/// Design-time factory so <c>dotnet ef</c> can build
/// <see cref="ModerationDbContext"/> without booting the API host. Never used at
/// runtime. Override the connection string with <c>MODERATION_DB_CONNECTION</c>
/// when generating provider-specific SQL.
/// </summary>
public sealed class ModerationDbContextFactory : IDesignTimeDbContextFactory<ModerationDbContext>
{
    private const string FallbackConnectionString =
        "Host=localhost;Port=5432;Database=storyverse_moderation;Username=postgres;Password=postgres";

    public ModerationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("MODERATION_DB_CONNECTION") ?? FallbackConnectionString;

        var options = new DbContextOptionsBuilder<ModerationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new ModerationDbContext(options);
    }
}
