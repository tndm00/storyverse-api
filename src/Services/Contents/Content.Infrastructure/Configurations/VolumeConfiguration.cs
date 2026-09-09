namespace Content.Infrastructure.Configurations;

public sealed class VolumeConfiguration : IEntityTypeConfiguration<Volume>
{
    public void Configure(EntityTypeBuilder<Volume> builder)
    {
        builder.ToTable(InfrastructureConstants.VolumesTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.StoryId, x.OrderIndex });

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(ApplicationConstants.MaxTitleLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOne<Story>()
            .WithMany()
            .HasForeignKey(x => x.StoryId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
