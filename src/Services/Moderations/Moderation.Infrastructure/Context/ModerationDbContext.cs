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

    /// <summary>The moderation reports table.</summary>
    public DbSet<Report> Reports => Set<Report>();

    /// <summary>The moderation action audit trail table.</summary>
    public DbSet<ModerationAction> ModerationActions => Set<ModerationAction>();

    /// <summary>Applies all <see cref="IEntityTypeConfiguration{TEntity}"/> found in this assembly.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table/column/index configuration lives in Configurations/*, not here
        // and not on the entity, per codebase-architecture-flow.md section 10.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
