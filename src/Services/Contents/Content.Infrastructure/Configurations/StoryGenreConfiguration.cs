namespace Content.Infrastructure.Configurations;

/// <summary>
/// Join configuration for <see cref="StoryGenre"/>. A filtered unique index keeps
/// at most one primary genre per story.
/// </summary>
public sealed class StoryGenreConfiguration : IEntityTypeConfiguration<StoryGenre>
{
    /// <summary>
    /// Configures the EF Core mapping for <see cref="StoryGenre"/>: composite key, the
    /// filtered unique index enforcing a single primary genre, and the FK to Genre.
    /// </summary>
    public void Configure(EntityTypeBuilder<StoryGenre> builder)
    {
        builder.ToTable(InfrastructureConstants.StoryGenresTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => new { x.StoryId, x.GenreId });

        // Filtered unique index: at most one row per story can be flagged IsPrimary.
        builder.HasIndex(x => x.StoryId)
            .HasFilter("\"IsPrimary\" = true")
            .IsUnique();

        // Genre rows are reference data; block deletion while stories still reference them.
        builder.HasOne(x => x.Genre)
            .WithMany()
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
