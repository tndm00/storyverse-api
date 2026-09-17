namespace Content.Infrastructure.Configurations;

public sealed class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    /// <summary>
    /// Configures the EF Core mapping for <see cref="Chapter"/>: table/schema, keys,
    /// indexes, column constraints and foreign key relationships.
    /// </summary>
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder.ToTable(InfrastructureConstants.ChaptersTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

        // Public id must be unique; the composite indexes speed up ordered chapter
        // listing per story and scheduled-publish lookups by status.
        builder.HasIndex(x => x.PublicId).IsUnique();
        builder.HasIndex(x => new { x.StoryId, x.OrderIndex });
        builder.HasIndex(x => new { x.Status, x.ScheduledAt });

        builder.Property(x => x.PublicId).IsRequired();

        builder.Property(x => x.Title)
            .HasMaxLength(ApplicationConstants.MaxChapterTitleLength)
            .IsRequired();

        builder.Property(x => x.OrderIndex)
            .HasColumnType("numeric(12,4)")
            .IsRequired();

        builder.Property(x => x.Content)
            .HasColumnType("text")
            .IsRequired();

        // Enums are persisted as strings for readability/portability.
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.RejectionReason)
            .HasMaxLength(2000);

        builder.Property(x => x.AccessType)
            .HasConversion<string>()
            .HasMaxLength(InfrastructureConstants.EnumColumnLength)
            .IsRequired();

        builder.Property(x => x.CreatedAt).IsRequired();

        // A chapter is removed when its owning story is hard-deleted, but only
        // detached (not deleted) from a volume when that volume is removed.
        builder.HasOne<Story>()
            .WithMany()
            .HasForeignKey(x => x.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Volume>()
            .WithMany()
            .HasForeignKey(x => x.VolumeId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
