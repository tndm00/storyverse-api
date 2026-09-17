namespace Content.Infrastructure.Configurations;

/// <summary>
/// Configuration for the <see cref="Tag"/> reference entity used to categorize stories.
/// </summary>
public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    /// <summary>
    /// Configures the EF Core mapping for <see cref="Tag"/>: table/schema, keys, indexes
    /// and column constraints.
    /// </summary>
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable(InfrastructureConstants.TagsTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

        // Name and slug must be unique; usage count is indexed for popularity sorting.
        builder.HasIndex(x => x.Name).IsUnique();
        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.UsageCount);

        builder.Property(x => x.Name)
            .HasMaxLength(ApplicationConstants.MaxTagNameLength)
            .IsRequired();

        builder.Property(x => x.Slug)
            .HasMaxLength(ApplicationConstants.MaxSlugLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();
    }
}
