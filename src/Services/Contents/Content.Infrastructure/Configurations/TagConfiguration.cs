namespace Content.Infrastructure.Configurations;

public sealed class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable(InfrastructureConstants.TagsTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

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
