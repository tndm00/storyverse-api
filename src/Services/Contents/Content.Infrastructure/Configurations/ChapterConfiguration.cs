namespace Content.Infrastructure.Configurations;

public sealed class ChapterConfiguration : IEntityTypeConfiguration<Chapter>
{
    public void Configure(EntityTypeBuilder<Chapter> builder)
    {
        builder.ToTable(InfrastructureConstants.ChaptersTableName, InfrastructureConstants.ContentSchemaName);

        builder.HasKey(x => x.Id);

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
