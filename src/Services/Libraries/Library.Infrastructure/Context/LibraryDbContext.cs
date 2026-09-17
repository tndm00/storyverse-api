namespace Library.Infrastructure.Context;

/// <summary>
/// EF Core context for the Library service. Only Infrastructure may
/// reference this type, per code-standard.md section 23 (DbContext Rules).
/// </summary>
public sealed class LibraryDbContext : DbContext
{
    public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
    {
    }

    public DbSet<LibraryEntry> LibraryEntries => Set<LibraryEntry>();

    public DbSet<ReadingProgress> ReadingProgress => Set<ReadingProgress>();

    /// <summary>Applies all <see cref="IEntityTypeConfiguration{TEntity}"/> mappings found in this assembly.</summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table/column/index configuration lives in Configurations/*, not here
        // and not on the entity, per codebase-architecture-flow.md section 10.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
