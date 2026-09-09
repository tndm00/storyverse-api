namespace Authentication.Infrastructure.Context;

/// <summary>
/// Design-time factory so <c>dotnet ef</c> can build
/// <see cref="AuthenticationDbContext"/> without booting the API host. Never
/// used at runtime. The connection string only resolves the Npgsql provider for
/// migration scaffolding; override it with <c>AUTH_DB_CONNECTION</c> when
/// generating provider-specific SQL.
/// </summary>
public sealed class AuthenticationDbContextFactory : IDesignTimeDbContextFactory<AuthenticationDbContext>
{
    private const string FallbackConnectionString =
        "Host=localhost;Port=5432;Database=storyverse_authentication;Username=postgres;Password=postgres";

    public AuthenticationDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("AUTH_DB_CONNECTION") ?? FallbackConnectionString;

        var options = new DbContextOptionsBuilder<AuthenticationDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new AuthenticationDbContext(options);
    }
}
