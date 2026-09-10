namespace Content.Infrastructure.Context;

/// <summary>
/// EF Core context for the Content service. Only Infrastructure may reference
/// this type, per code-standard.md section 23 (DbContext Rules).
/// </summary>
public sealed class ContentDbContext : DbContext
{
    public ContentDbContext(DbContextOptions<ContentDbContext> options) : base(options)
    {
    }

    public DbSet<Story> Stories => Set<Story>();

    public DbSet<Volume> Volumes => Set<Volume>();

    public DbSet<Chapter> Chapters => Set<Chapter>();

    public DbSet<ChapterReviewAction> ChapterReviewActions => Set<ChapterReviewAction>();

    public DbSet<Genre> Genres => Set<Genre>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<StoryGenre> StoryGenres => Set<StoryGenre>();

    public DbSet<StoryTag> StoryTags => Set<StoryTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table/column/index configuration lives in Configurations/*, not here
        // and not on the entity, per codebase-architecture-flow.md section 10.
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
