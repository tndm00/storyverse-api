namespace Moderation.Infrastructure.Context;

/// <summary>
/// EF Core context for the Moderation service. Only Infrastructure may
/// reference this type, per code-standard.md section 23 (DbContext Rules).
/// </summary>
public sealed class ModerationDbContext : DbContext
{
    public ModerationDbContext(DbContextOptions<ModerationDbContext> options) : base(options)
    {
    }

    public DbSet<Report> Reports => Set<Report>();

    public DbSet<ModerationAction> ModerationActions => Set<ModerationAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table/column/index configuration lives in Configurations/*, not here
        // and not on the entity, per codebase-architecture-flow.md section 10.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
