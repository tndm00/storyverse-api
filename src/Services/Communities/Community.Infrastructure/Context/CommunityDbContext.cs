namespace Community.Infrastructure.Context;

/// <summary>
/// EF Core context for the Community service. Only Infrastructure may
/// reference this type, per code-standard.md section 23 (DbContext Rules).
/// </summary>
public sealed class CommunityDbContext : DbContext
{
    /// <summary>Creates the context with the options supplied by DI (connection string, provider, etc.).</summary>
    public CommunityDbContext(DbContextOptions<CommunityDbContext> options) : base(options)
    {
    }

    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Rating> Ratings => Set<Rating>();

    public DbSet<Vote> Votes => Set<Vote>();

    /// <summary>Applies all <see cref="Configurations"/> entity configurations from this assembly to the model.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table/column/index configuration lives in Configurations/*, not here
        // and not on the entity, per codebase-architecture-flow.md section 10.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
