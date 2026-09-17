namespace Library.Infrastructure.Context;

/// <summary>
/// Design-time factory so <c>dotnet ef</c> can build <see cref="LibraryDbContext"/>
/// without booting the API host. Never used at runtime. The connection string is
/// only used to resolve the Npgsql provider for migration scaffolding; override it
/// with <c>LIBRARY_DB_CONNECTION</c> when generating provider-specific SQL.
/// </summary>
public sealed class LibraryDbContextFactory : IDesignTimeDbContextFactory<LibraryDbContext>
{
    private const string FallbackConnectionString =
        "Host=localhost;Port=5432;Database=storyverse_library;Username=postgres;Password=postgres";

    /// <summary>Builds a <see cref="LibraryDbContext"/> for design-time tooling (e.g. <c>dotnet ef</c>).</summary>
    public LibraryDbContext CreateDbContext(string[] args)
    {
        // Prefer an explicit override so migration SQL can target the right provider/environment.
        var connectionString = Environment.GetEnvironmentVariable("LIBRARY_DB_CONNECTION") ?? FallbackConnectionString;

        var options = new DbContextOptionsBuilder<LibraryDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new LibraryDbContext(options);
    }
}
