namespace Content.Infrastructure.Context;

/// <summary>
/// EF Core context for the Content service. Only Infrastructure may
/// reference this type, per code-standard.md section 23 (DbContext Rules).
/// Add DbSet properties here as Content.Domain entities are introduced.
/// </summary>
public sealed class ContentDbContext : DbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table/column/index configuration lives in Configurations/*, not here
        // and not on the entity, per codebase-architecture-flow.md section 10.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
