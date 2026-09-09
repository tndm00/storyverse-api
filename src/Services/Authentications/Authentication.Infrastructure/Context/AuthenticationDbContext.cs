namespace Authentication.Infrastructure.Context;

/// <summary>
/// EF Core context for the Authentication service. Only Infrastructure may
/// reference this type, per code-standard.md section 23 (DbContext Rules).
/// </summary>
public sealed class AuthenticationDbContext : DbContext
{
    public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<AuthorProfile> AuthorProfiles => Set<AuthorProfile>();

    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table/column/index configuration lives in Configurations/*, not here
        // and not on the entity, per codebase-architecture-flow.md section 10.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
