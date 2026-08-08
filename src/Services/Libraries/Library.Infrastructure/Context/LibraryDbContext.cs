namespace Library.Infrastructure.Context;

/// <summary>
/// EF Core context for the Library service. Only Infrastructure may
/// reference this type, per code-standard.md section 23 (DbContext Rules).
/// Add DbSet properties here as Library.Domain entities are introduced.
/// </summary>
public sealed class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
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
