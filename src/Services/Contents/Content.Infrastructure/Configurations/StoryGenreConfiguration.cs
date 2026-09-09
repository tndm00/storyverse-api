namespace Content.Infrastructure.Configurations;

/// <summary>
/// Join configuration for <see cref="StoryGenre"/>. A filtered unique index keeps
/// at most one primary genre per story.
/// </summary>
public sealed class StoryGenreConfiguration : IEntityTypeConfiguration<StoryGenre>
{
    public void Configure(EntityTypeBuilder<StoryGenre> builder)
    {
        builder.ToTable(InfrastructureConstants.StoryGenresTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => new { x.StoryId, x.GenreId });

        builder.HasIndex(x => x.StoryId)
            .HasFilter("\"IsPrimary\" = true")
            .IsUnique();

        builder.HasOne(x => x.Genre)
            .WithMany()
            .HasForeignKey(x => x.GenreId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
