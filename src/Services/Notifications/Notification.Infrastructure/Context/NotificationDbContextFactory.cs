namespace Notification.Infrastructure.Context;

/// <summary>
/// Design-time factory so <c>dotnet ef</c> can build <see cref="NotificationDbContext"/>
/// without booting the API host. Never used at runtime. The connection string is
/// only used to resolve the Npgsql provider for migration scaffolding; override it
/// with <c>NOTIFICATION_DB_CONNECTION</c> when generating provider-specific SQL.
/// </summary>
public sealed class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    private const string FallbackConnectionString =
        "Host=localhost;Port=5432;Database=storyverse_notification;Username=postgres;Password=postgres";

    /// <summary>Builds a <see cref="NotificationDbContext"/> for design-time tooling (e.g. migrations).</summary>
    public NotificationDbContext CreateDbContext(string[] args)
    {
        // Prefer the override env var (used for provider-specific SQL generation) over the local fallback.
        var connectionString =
            Environment.GetEnvironmentVariable(InfrastructureConstants.DesignTimeConnectionEnvVar)
            ?? FallbackConnectionString;

        var options = new DbContextOptionsBuilder<NotificationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new NotificationDbContext(options);
    }
}
