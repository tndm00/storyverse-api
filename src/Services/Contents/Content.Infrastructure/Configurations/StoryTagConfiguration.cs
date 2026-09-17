namespace Content.Infrastructure.Configurations;

/// <summary>
/// Join configuration for <see cref="StoryTag"/>, the many-to-many link between stories and tags.
/// </summary>
public sealed class StoryTagConfiguration : IEntityTypeConfiguration<StoryTag>
{
    /// <summary>
    /// Configures the EF Core mapping for <see cref="StoryTag"/>: composite key, the
    /// lookup index on tag id, and the FK to Tag.
    /// </summary>
    public void Configure(EntityTypeBuilder<StoryTag> builder)
    {
        builder.ToTable(InfrastructureConstants.StoryTagsTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => new { x.StoryId, x.TagId });

        builder.HasIndex(x => x.TagId);

        // Tag rows are reference data; block deletion while stories still reference them.
        builder.HasOne(x => x.Tag)
            .WithMany()
            .HasForeignKey(x => x.TagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
