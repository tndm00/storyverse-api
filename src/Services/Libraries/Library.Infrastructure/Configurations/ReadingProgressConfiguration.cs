namespace Library.Infrastructure.Configurations;

/// <summary>EF Core mapping for <see cref="ReadingProgress"/>: table, keys, indexes and column constraints.</summary>
public sealed class ReadingProgressConfiguration : IEntityTypeConfiguration<ReadingProgress>
{
    /// <summary>Configures the table mapping, indexes and column constraints for <see cref="ReadingProgress"/>.</summary>
    public void Configure(EntityTypeBuilder<ReadingProgress> builder)
    {
        builder.ToTable(InfrastructureConstants.ReadingProgressTableName, InfrastructureConstants.LibrarySchemaName);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PublicId).IsUnique();

        // One progress row per (reader, story) (domain spec unique(user_id, story_id)).
        builder.HasIndex(x => new { x.UserId, x.StoryId }).IsUnique();

        // "Continue reading" lists a user's rows most-recently-read first.
        builder.HasIndex(x => new { x.UserId, x.LastReadAt });

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.UserId).IsRequired();

        builder.Property(x => x.StoryId).IsRequired();

        builder.Property(x => x.LastChapterId).IsRequired();

        builder.Property(x => x.ScrollPercent)
            .HasColumnType("numeric(5,2)");

        builder.Property(x => x.LastReadAt).IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
