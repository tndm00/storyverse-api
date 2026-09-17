namespace Content.Infrastructure.Configurations;

/// <summary>
/// Configuration for the <see cref="Volume"/> entity, which groups a story's chapters
/// into ordered sections.
/// </summary>
public sealed class VolumeConfiguration : IEntityTypeConfiguration<Volume>
{
    /// <summary>
    /// Configures the EF Core mapping for <see cref="Volume"/>: table/schema, keys, indexes,
    /// column constraints and the cascade relationship to <see cref="Story"/>.
    /// </summary>
    public void Configure(EntityTypeBuilder<Volume> builder)
    {
        builder.ToTable(InfrastructureConstants.VolumesTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

        // Public id must be unique; the composite index speeds up ordered volume listing per story.
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.StoryId, x.OrderIndex });

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(ApplicationConstants.MaxTitleLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        // A volume is removed when its owning story is hard-deleted.
        builder.HasOne<Story>()
            .WithMany()
            .HasForeignKey(x => x.StoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
